namespace Monolith.Sentinel.Services;

public sealed class SentinelOptions
{
    public string MonolithBaseUrl { get; set; } = "http://localhost:65000";
    public int DependencyTimeoutSeconds { get; set; } = 3;
}