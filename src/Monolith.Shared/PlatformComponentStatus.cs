namespace Monolith.Shared;

public sealed record PlatformComponentStatus(string Name, string Status, string Route);

public static class PlatformComponentCatalog
{
    public static IReadOnlyList<PlatformComponentStatus> All { get; } =
        new PlatformComponentStatus[]
        {
            new("Data", "available", "/api/data/status"),
            new("Cache", "available", "/api/cache/status"),
            new("Events", "available", "/api/events/status"),
            new("Integration", "available", "/api/integration/status"),
            new("Analytics", "available", "/api/analytics/status")
        };
}