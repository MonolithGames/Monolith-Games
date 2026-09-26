using System.Text.Json;

namespace Monolith.Services;

public sealed class BalanceSyncService(IWebHostEnvironment environment)
{
    private readonly string _filePath = Path.Combine(environment.ContentRootPath, "App_Data", "balance.json");

    public async Task UpdateBalanceAsync(decimal totalBalance, CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var data = new { Balance = totalBalance, UpdatedAtUtc = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        catch
        {
            // Suppress IO errors during background ticks
        }
    }
}
