namespace Monolith.Shared.Features;

public sealed class FeatureRegistry
{
    private readonly object _sync = new();
    private readonly List<FeatureIdentity> _features;

    public FeatureRegistry()
    {
        _features = PortRegistry.FeatureNamespaces
            .SelectMany(featureNamespace => Enumerable.Range(featureNamespace.Start, featureNamespace.Size)
                .Select(port =>
                {
                    var category = Enum.Parse<FeatureCategory>(featureNamespace.Name);
                    var featureNumber = port - featureNamespace.Start + 1;
                    var featureId = $"{category.ToString().ToUpperInvariant()}-{featureNumber:000}";
                    var name = $"Feature{featureNumber:000}";
                    var description = $"Unallocated {featureNamespace.Name} feature identity {featureId}.";
                    return new FeatureIdentity(port, featureId, category, name, description);
                }))
            .ToList();
    }

    public IReadOnlyList<FeatureMetadata> GetAll()
    {
        lock (_sync)
        {
            return _features.Select(feature => feature.ToMetadata()).ToArray();
        }
    }

    public FeatureMetadata? GetByPort(int port)
    {
        lock (_sync)
        {
            return _features.FirstOrDefault(feature => feature.PortNumber == port)?.ToMetadata();
        }
    }

    public FeatureMetadata? GetByName(string name)
    {
        lock (_sync)
        {
            return _features.FirstOrDefault(feature =>
                string.Equals(feature.Name, name, StringComparison.OrdinalIgnoreCase))?.ToMetadata();
        }
    }

    public IReadOnlyList<FeatureMetadata> GetByCategory(FeatureCategory category)
    {
        lock (_sync)
        {
            return _features.Where(feature => feature.Category == category)
                .Select(feature => feature.ToMetadata()).ToArray();
        }
    }

    public IReadOnlyList<FeatureMetadata> GetByStatus(FeatureStatus status)
    {
        lock (_sync)
        {
            return _features.Where(feature => feature.Status == status)
                .Select(feature => feature.ToMetadata()).ToArray();
        }
    }

    public FeatureMetadata Assign(int port, string name, string description)
    {
        ValidateText(name, nameof(name));
        ValidateText(description, nameof(description));

        lock (_sync)
        {
            var feature = FindByPort(port);
            if (feature.Status is FeatureStatus.Active or FeatureStatus.Deprecated)
            {
                throw new InvalidOperationException($"Feature {port} cannot be assigned in its current state.");
            }

            if (_features.Any(existing =>
                    existing.PortNumber != port &&
                    string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Feature name '{name}' is already assigned.");
            }

            return feature.Assign(name, description);
        }
    }

    public FeatureMetadata Rename(int port, string name)
    {
        ValidateText(name, nameof(name));

        lock (_sync)
        {
            var feature = FindByPort(port);
            if (_features.Any(existing =>
                    existing.PortNumber != port &&
                    string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Feature name '{name}' is already assigned.");
            }

            return feature.Rename(name);
        }
    }

    private FeatureIdentity FindByPort(int port) =>
        _features.FirstOrDefault(feature => feature.PortNumber == port)
        ?? throw new KeyNotFoundException($"Feature port {port} is not in the feature identity space.");

    private static void ValidateText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }
    }
}