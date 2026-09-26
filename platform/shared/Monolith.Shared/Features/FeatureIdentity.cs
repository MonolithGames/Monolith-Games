namespace Monolith.Shared.Features;

public sealed class FeatureIdentity
{
    public FeatureIdentity(
        int portNumber,
        string featureId,
        FeatureCategory category,
        string name,
        string description,
        FeatureStatus status = FeatureStatus.Unallocated)
    {
        PortNumber = portNumber;
        FeatureId = featureId;
        Category = category;
        Name = name;
        Description = description;
        Status = status;
    }

    public int PortNumber { get; }

    public string FeatureId { get; }

    public FeatureCategory Category { get; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public FeatureStatus Status { get; private set; }

    internal FeatureMetadata Assign(string name, string description)
    {
        Name = name;
        Description = description;
        Status = FeatureStatus.Assigned;
        return ToMetadata();
    }

    internal FeatureMetadata Rename(string name)
    {
        Name = name;
        return ToMetadata();
    }

    internal FeatureMetadata ToMetadata() =>
        new(PortNumber, FeatureId, Category, Name, Description, Status);
}