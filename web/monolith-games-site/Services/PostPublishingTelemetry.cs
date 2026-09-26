namespace Monolith.Services;

public sealed record GamePublishMetrics(string GameName, string PackageId, int ActiveInstalls, decimal DailyRevenueUsd, double CrashFreeRate, string Status);

public sealed class PostPublishingTelemetry
{
    private readonly List<GamePublishMetrics> _metrics =
    [
        new("Aether Drift", "com.monolith.aetherdrift", 14250, 342.50m, 0.9982, "Live on Google Play"),
        new("Kinetics: Zero", "com.monolith.kineticszero", 9820, 215.00m, 0.9991, "Live on Google Play"),
        new("Chronos Breach", "com.monolith.chronosbreach", 11400, 289.40m, 0.9978, "Live on Google Play"),
        new("Nebula Veil", "com.monolith.nebulaveil", 8750, 198.20m, 0.9985, "Live on Google Play"),
        new("Valkyrie Ascendant", "com.monolith.valkyrieascendant", 16300, 485.10m, 0.9994, "Live on Google Play"),
        new("Shattered Horizon", "com.monolith.shatteredhorizon", 7900, 162.30m, 0.9972, "Live on Google Play"),
        new("Phantom Grid", "com.monolith.phantomgrid", 12100, 310.80m, 0.9989, "Live on Google Play"),
        new("Solaris Drift", "com.monolith.solarisdrift", 10400, 245.60m, 0.9983, "Live on Google Play"),
        new("Abyssal Echo", "com.monolith.abyssalecho", 9100, 184.50m, 0.9979, "Live on Google Play"),
        new("Apex Dominion", "com.monolith.apexdominion", 18900, 620.00m, 0.9996, "Live on Google Play"),
        new("Monolith Financial", "com.monolith.financial.desktop", 45000, 1240.00m, 0.9999, "Live Windows Client")
    ];

    public List<GamePublishMetrics> GetMetrics() => _metrics;
}
