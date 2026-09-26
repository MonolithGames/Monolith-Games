namespace Monolith.Sentinel.Services;

public sealed class PlatformState
{
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;

    public const string ServiceName = "Monolith.Sentinel";
    public const string Version = "1.0.0";

    public DateTimeOffset StartedAt => _startedAt;

    public double UptimeSeconds => (DateTimeOffset.UtcNow - _startedAt).TotalSeconds;
}
