using System.Collections.Concurrent;
using System.Text.Json;
using Monolith.Shared;

namespace Monolith.Data;

public sealed class DataService(DataOptions? options = null) : IDataProvider
{
    private readonly ConcurrentDictionary<string, DataRecord> _records = new(StringComparer.Ordinal);
    private readonly DataOptions _options = options ?? new DataOptions();
    private readonly ConcurrentDictionary<string, DataItem> _items = new(StringComparer.Ordinal);

    public DataRecord? Get(string key) => _records.GetValueOrDefault(key);

    public void Save(DataRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (!_records.ContainsKey(record.Key) && _records.Count >= _options.MaximumRecords)
        {
            throw new InvalidOperationException("The in-memory data capacity has been reached.");
        }

        _records[record.Key] = record;
    }

    public bool Remove(string key) => _records.TryRemove(key, out _);

    public PlatformComponentStatus GetStatus() =>
        new("Data", "available", "/api/data/status");

    public int Count => _items.Count;

    public ValueTask<DataItem> UpsertAsync(string key, JsonElement value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateKey(key);
        var now = DateTimeOffset.UtcNow;
        var clone = value.Clone();
        var item = _items.AddOrUpdate(key,
            _ => new DataItem(key, clone, now, now, 1),
            (_, existing) => new DataItem(key, clone, existing.CreatedUtc, now, existing.Version + 1));
        if (_items.Count > _options.MaximumRecords)
        {
            _items.TryRemove(key, out _);
            throw new InvalidOperationException("The in-memory data capacity has been reached.");
        }

        return ValueTask.FromResult(item);
    }

    public ValueTask<DataItem?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateKey(key);
        return ValueTask.FromResult(_items.TryGetValue(key, out var item) ? item : null);
    }

    public ValueTask<IReadOnlyList<DataItem>> ListAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (offset < 0 || limit < 1 || limit > _options.MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Pagination is outside the allowed bounds.");
        }

        return ValueTask.FromResult<IReadOnlyList<DataItem>>(_items.Values
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Skip(offset).Take(limit).ToArray());
    }

    public ValueTask<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateKey(key);
        return ValueTask.FromResult(_items.TryRemove(key, out _));
    }

    private void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > _options.MaximumKeyLength)
        {
            throw new ArgumentException("The key is required and exceeds the allowed length.", nameof(key));
        }
    }
}