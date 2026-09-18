namespace Monolith.Shared;

public sealed record FeatureStatus(
    int Identity,
    string Name,
    string State,
    DateTimeOffset TimestampUtc,
    IReadOnlyDictionary<string, object?> Details);