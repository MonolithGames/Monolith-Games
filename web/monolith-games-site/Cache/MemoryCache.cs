using System.Collections.Concurrent;
using System.Text.Json;

namespace Monolith.Cache;

public sealed class MemoryCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, CacheItem> _jsonEntries = new(StringComparer.Ordinal);
    private long _hits;
    private long _misses;

    public T? Get<T>(string key)
    {
        if (!_entries.TryGetValue(key, out var entry))
        {
            return default;
        }

        if (entry.ExpiresAtUtc is not null && entry.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(key, out _);
            return default;
        }

        return entry.Value is T value ? value : default;
    }

    public void Set<T>(string key, T value, TimeSpan? lifetime)
    {
        DateTimeOffset? expiresAt = lifetime is null ? null : DateTimeOffset.UtcNow.Add(lifetime.Value);
        _entries[key] = new CacheEntry(key, value!, expiresAt);
    }

    public bool Remove(string key) => _entries.TryRemove(key, out _);

    public bool Contains(string key) => Get<object>(key) is not null;

    public CacheStatistics GetStatistics() => new(Interlocked.Read(ref _hits), Interlocked.Read(ref _misses), _jsonEntries.Count);

    public ValueTask SetJsonAsync(string key, JsonElement value, TimeSpan? lifetime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128)
        {
            throw new ArgumentException("The cache key is required and must be at most 128 characters.", nameof(key));
        }
        if (lifetime is { } duration && (duration <= TimeSpan.Zero || duration > TimeSpan.FromDays(7)))
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Cache expiration must be between one second and seven days.");
        }

        var now = DateTimeOffset.UtcNow;
        _jsonEntries[key] = new CacheItem(key, value.Clone(), now, lifetime is null ? null : now.Add(lifetime.Value));
        return ValueTask.CompletedTask;
    }

    public ValueTask<JsonElement?> GetJsonAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_jsonEntries.TryGetValue(key, out var entry) || entry.ExpiresAtUtc is not null && entry.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            Interlocked.Increment(ref _misses);
            _jsonEntries.TryRemove(key, out _);
            return ValueTask.FromResult<JsonElement?>(null);
        }

        Interlocked.Increment(ref _hits);
        return ValueTask.FromResult<JsonElement?>(entry.Value.Clone());
    }

    public ValueTask<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(_jsonEntries.TryRemove(key, out _));
    }

    public int PurgeExpired()
    {
        var now = DateTimeOffset.UtcNow;
        var removed = 0;
        foreach (var pair in _jsonEntries.Where(pair => pair.Value.ExpiresAtUtc <= now).ToArray())
        {
            if (_jsonEntries.TryRemove(pair.Key, out _)) removed++;
        }
        return removed;
    }
}