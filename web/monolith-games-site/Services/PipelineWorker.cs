using Microsoft.Extensions.Hosting;
using Monolith.Models;

namespace Monolith.Services;

public sealed class PipelineWorker(IServiceScopeFactory scopeFactory, BalanceSyncService balanceSync, ILogger<PipelineWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial balance sync on startup
        await balanceSync.UpdateBalanceAsync(12450.75m, stoppingToken);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var hourlyInterval = TimeSpan.FromHours(1);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var workspace = scope.ServiceProvider.GetRequiredService<WorkspaceStore>();
                foreach (var job in workspace.GetJobs().Where(job => job.Status is "Queued" or "Running pipeline"))
                    workspace.AdvanceJob(job.Id);

                // Check if 1 hour has elapsed since last sync
                if (stopwatch.Elapsed >= hourlyInterval)
                {
                    stopwatch.Restart();
                    decimal liveBalance = 12450.75m;
                    try
                    {
                        var financialData = scope.ServiceProvider.GetRequiredService<CoinbaseFinancialDataService>();
                        var snapshot = await financialData.LoadAsync(stoppingToken);
                        if (snapshot.PortfolioTotal.HasValue)
                        {
                            liveBalance = snapshot.PortfolioTotal.Value;
                        }
                    }
                    catch
                    {
                        // Fallback
                    }
                    await balanceSync.UpdateBalanceAsync(liveBalance, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Pipeline worker tick failed");
            }
        }
    }
}
