using System.Text.Json;
using Monolith.Shared;

namespace Monolith.Cache;

public sealed class CacheService(MemoryCache cache) : ICacheProvider
{
    public T? Get<T>(string key) => cache.Get<T>(key);
    public void Set<T>(string key, T value, TimeSpan? lifetime = null) => cache.Set(key, value, lifetime);
    public bool Remove(string key) => cache.Remove(key);
    public bool Contains(string key) => cache.Contains(key);

    public PlatformComponentStatus GetStatus() =>
        new("Cache", "available", "/api/cache/status");

    public CacheStatistics GetStatistics() => cache.GetStatistics();
    public ValueTask SetJsonAsync(string key, JsonElement value, TimeSpan? lifetime, CancellationToken cancellationToken = default) => cache.SetJsonAsync(key, value, lifetime, cancellationToken);
    public ValueTask<JsonElement?> GetJsonAsync(string key, CancellationToken cancellationToken = default) => cache.GetJsonAsync(key, cancellationToken);
    public ValueTask<bool> RemoveAsync(string key, CancellationToken cancellationToken = default) => cache.RemoveAsync(key, cancellationToken);
    public int PurgeExpired() => cache.PurgeExpired();
}