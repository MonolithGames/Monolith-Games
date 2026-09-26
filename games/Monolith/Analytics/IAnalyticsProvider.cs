using Monolith.Shared;

namespace Monolith.Analytics;

public sealed record MetricRecord(string Name, double Value, DateTimeOffset RecordedAtUtc);
public sealed record MetricCollection(IReadOnlyList<MetricRecord> Metrics);
public sealed record AnalyticsSummary(DateTimeOffset StartedUtc, IReadOnlyDictionary<string, long> Counters);

public interface IAnalyticsProvider
{
    void Increment(string name, double amount = 1);
    void Record(string name, double value);
    MetricCollection GetMetrics();

    PlatformComponentStatus GetStatus();
    void IncrementBounded(string name, long amount = 1);
    AnalyticsSummary GetSummary();
}