using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using Monolith.Shared;

namespace Monolith.Events;

public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<string, List<IEventHandler>> _handlers = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<EventEnvelope> _recent = new();
    private readonly ILogger<EventBus>? _logger;
    private readonly int _historyLimit = 100;
    private long _published;
    private long _handled;
    private long _failed;

    public EventBus(ILogger<EventBus>? logger = null)
    {
        _logger = logger;
    }

    public void Subscribe(string eventName, IEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlers.AddOrUpdate(eventName,
            _ => [handler],
            (_, existing) =>
            {
                lock (existing)
                {
                    existing.Add(handler);
                    return existing;
                }
            });
    }

    public async Task PublishAsync(EventMessage message, CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(message.Name, out var handlers))
        {
            return;
        }

        IEventHandler[] snapshot;
        lock (handlers)
        {
            snapshot = handlers.ToArray();
        }

        foreach (var handler in snapshot)
        {
            await handler.HandleAsync(message, cancellationToken);
        }
    }

    public PlatformComponentStatus GetStatus() =>
        new("Events", "available", "/api/events/status");

    public async ValueTask PublishAsync(EventEnvelope message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _recent.Enqueue(message);
        while (_recent.Count > _historyLimit) _recent.TryDequeue(out _);
        Interlocked.Increment(ref _published);
        if (!_handlers.TryGetValue(message.EventType, out var handlers)) return;
        IEventHandler[] snapshot;
        lock (handlers) snapshot = handlers.ToArray();
        foreach (var handler in snapshot)
        {
            try
            {
                await handler.HandleAsync(new EventMessage(message.EventType, message.Payload, message.OccurredUtc), cancellationToken);
                Interlocked.Increment(ref _handled);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Interlocked.Increment(ref _failed);
                _logger?.LogError(exception, "Event handler failed for {EventType} {EventId}", message.EventType, message.EventId);
            }
        }
    }

    public IReadOnlyList<EventEnvelope> GetRecent(int limit) =>
        _recent.Reverse().Take(Math.Clamp(limit, 1, _historyLimit)).ToImmutableArray();

    public EventStatistics GetStatistics() => new(Interlocked.Read(ref _published), Interlocked.Read(ref _handled), Interlocked.Read(ref _failed), _recent.Count);
}