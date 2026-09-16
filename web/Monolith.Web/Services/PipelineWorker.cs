using Microsoft.Extensions.Hosting;
using Monolith.Web.Models;

namespace Monolith.Web.Services;

public sealed class PipelineWorker(IServiceScopeFactory scopeFactory, ILogger<PipelineWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var workspace = scope.ServiceProvider.GetRequiredService<WorkspaceStore>();
                foreach (var job in workspace.GetJobs().Where(job => job.Status is "Queued" or "Running pipeline"))
                    workspace.AdvanceJob(job.Id);
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
