using Monolith.Shared;

namespace Monolith.Events;

public sealed record EventMessage(string Name, object? Payload, DateTimeOffset PublishedAtUtc);
public sealed record EventEnvelope(Guid EventId, string EventType, string CorrelationId, string Source, DateTimeOffset OccurredUtc, System.Text.Json.JsonElement Payload);
public sealed record EventStatistics(long Published, long Handled, long Failed, int RecentCount);

public interface IEventHandler
{
    Task HandleAsync(EventMessage message, CancellationToken cancellationToken = default);
}

public interface IEventBus
{
    void Subscribe(string eventName, IEventHandler handler);
    Task PublishAsync(EventMessage message, CancellationToken cancellationToken = default);

    PlatformComponentStatus GetStatus();
    ValueTask PublishAsync(EventEnvelope message, CancellationToken cancellationToken = default);
    IReadOnlyList<EventEnvelope> GetRecent(int limit);
    EventStatistics GetStatistics();
}