namespace Monolith.Shared.Features;

public enum FeatureStatus
{
    Unallocated,
    Planned,
    Assigned,
    Active,
    Deprecated
}

public sealed record FeatureMetadata(
    int PortNumber,
    string FeatureId,
    FeatureCategory Category,
    string Name,
    string Description,
    FeatureStatus Status);