using System.Collections.Concurrent;
using Monolith.Shared;

namespace Monolith.Analytics;

public sealed class AnalyticsService : IAnalyticsProvider
{
    private readonly ConcurrentDictionary<string, double> _counters = new(StringComparer.Ordinal);
    private readonly DateTimeOffset _startedUtc = DateTimeOffset.UtcNow;

    public void Increment(string name, double amount = 1) => _counters.AddOrUpdate(name, amount, (_, value) => value + amount);
    public void Record(string name, double value) => _counters[name] = value;

    public MetricCollection GetMetrics() => new(_counters
        .OrderBy(metric => metric.Key)
        .Select(metric => new MetricRecord(metric.Key, metric.Value, DateTimeOffset.UtcNow))
        .ToArray());

    public PlatformComponentStatus GetStatus() =>
        new("Analytics", "available", "/api/analytics/status");

    public void IncrementBounded(string name, long amount = 1)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 64) return;
        _counters.AddOrUpdate(name, amount, (_, value) => value + amount);
    }

    public AnalyticsSummary GetSummary() => new(
        _startedUtc,
        _counters.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => (long)pair.Value));
}