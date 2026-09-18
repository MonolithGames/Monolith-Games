namespace Monolith.Shared;

public sealed record PortMetadata(int Port, string Name, string Purpose, string Status);

public sealed class PlatformRegistry
{
    private readonly List<PortDefinition> _ports = new(PortRegistry.OfficialPorts);

    public int NamespaceStart => PortRegistry.NamespaceStart;

    public int NamespaceEnd => PortRegistry.NamespaceEnd;

    public IReadOnlyList<PortDefinition> Ports => _ports.AsReadOnly();

    public IReadOnlyList<FeatureNamespaceDefinition> FeatureNamespaces => PortRegistry.FeatureNamespaces;

    public IReadOnlyList<FeatureIdentity> AvailableFeatureIdentities =>
        PortRegistry.FeatureIdentities.Where(identity => _ports.All(port => port.Port != identity.Port)).ToArray();

    public int AvailablePortCount => AvailableFeatureIdentities.Count;

    public void Register(PortDefinition port)
    {
        ArgumentNullException.ThrowIfNull(port);

        if (port.Port < NamespaceStart || port.Port > NamespaceEnd)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "The port must be within the Monolith namespace.");
        }

        if (_ports.Any(existing => existing.Port == port.Port))
        {
            throw new InvalidOperationException($"Port {port.Port} is already registered.");
        }

        _ports.Add(port);
    }

    public PortMetadata? GetPort(int port)
    {
        var definition = _ports.FirstOrDefault(existing => existing.Port == port);
        return definition is null
            ? null
            : new PortMetadata(definition.Port, definition.Name, definition.Purpose, "listening");
    }
}