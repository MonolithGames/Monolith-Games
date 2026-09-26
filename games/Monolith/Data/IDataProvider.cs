using System.Text.Json;
using Monolith.Shared;

namespace Monolith.Data;

public sealed record DataRecord(string Key, string Value, DateTimeOffset UpdatedAtUtc);
public sealed record DataItem(string Key, JsonElement Value, DateTimeOffset CreatedUtc, DateTimeOffset UpdatedUtc, long Version);

public interface IDataProvider
{
    DataRecord? Get(string key);
    void Save(DataRecord record);
    bool Remove(string key);

    PlatformComponentStatus GetStatus();

    ValueTask<DataItem> UpsertAsync(string key, JsonElement value, CancellationToken cancellationToken = default);
    ValueTask<DataItem?> GetAsync(string key, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<DataItem>> ListAsync(int offset, int limit, CancellationToken cancellationToken = default);
    ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
    int Count { get; }
}