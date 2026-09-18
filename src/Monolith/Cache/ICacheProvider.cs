using System.Text.Json;
using Monolith.Shared;

namespace Monolith.Cache;

public sealed record CacheEntry(string Key, object Value, DateTimeOffset? ExpiresAtUtc);
public sealed record CacheItem(string Key, JsonElement Value, DateTimeOffset CreatedUtc, DateTimeOffset? ExpiresAtUtc);
public sealed record CacheStatistics(long Hits, long Misses, int Entries);

public interface ICacheProvider
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? lifetime = null);
    bool Remove(string key);
    bool Contains(string key);

    PlatformComponentStatus GetStatus();
    CacheStatistics GetStatistics();
    ValueTask SetJsonAsync(string key, JsonElement value, TimeSpan? lifetime, CancellationToken cancellationToken = default);
    ValueTask<JsonElement?> GetJsonAsync(string key, CancellationToken cancellationToken = default);
    ValueTask<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
    int PurgeExpired();
}