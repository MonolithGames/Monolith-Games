namespace Monolith.Shared;

public sealed record PortDefinition(int Port, string Name, string Purpose);

public sealed record FeatureNamespaceDefinition(string Name, int Start, int End)
{
    public int Size => End - Start + 1;
}

public sealed record FeatureIdentity(
    int Port,
    string Name,
    string Namespace,
    int FeatureNumber,
    string Status);

public sealed class PortRegistry
{
    public const int Monolith = 65000;
    public const int Sentinel = 65535;
    public const int NamespaceStart = Monolith;
    public const int NamespaceEnd = Sentinel;

    public const int FeatureIdentityStart = Monolith + 1;
    public const int FeatureIdentityEnd = Sentinel - 1;
    public const int FeatureNamespaceSize = 89;

    public static IReadOnlyList<FeatureNamespaceDefinition> FeatureNamespaces { get; } =
        new FeatureNamespaceDefinition[]
        {
            new("Platform", 65001, 65089),
            new("Data", 65090, 65178),
            new("Integration", 65179, 65267),
            new("Operations", 65268, 65356),
            new("Security", 65357, 65445),
            new("Experience", 65446, 65534)
        };

    public static IReadOnlyList<PortDefinition> OfficialPorts { get; } =
        new PortDefinition[]
        {
            new(Monolith, "Monolith", "Primary Monolith application namespace entry point."),
            new(Sentinel, "Sentinel", "Authoritative discovery and health endpoint for the Monolith namespace.")
        };

    public static IReadOnlyList<FeatureIdentity> FeatureIdentities { get; } =
        FeatureNamespaces
            .SelectMany(featureNamespace => Enumerable.Range(featureNamespace.Start, featureNamespace.Size)
                .Select(port => new FeatureIdentity(
                    port,
                    $"Monolith.{featureNamespace.Name}.Feature{port - featureNamespace.Start + 1:000}",
                    featureNamespace.Name,
                    port - featureNamespace.Start + 1,
                    "unallocated")))
            .ToArray();
}
