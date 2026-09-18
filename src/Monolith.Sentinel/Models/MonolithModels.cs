namespace Monolith.Sentinel.Models;

public sealed record HealthResponse(string Status, DateTimeOffset TimestampUtc);

public sealed record StatusResponse(
    string Service,
    string Status,
    string Version,
    string Environment,
    string MachineName,
    double UptimeSeconds);

public sealed record VersionResponse(string Service, string Version);

public sealed record TimeResponse(DateTimeOffset Utc);

public sealed record ComponentInfo(string Name, string Status, string Route, string? Error = null);

public sealed record ComponentsResponse(IReadOnlyList<ComponentInfo> Components);

public sealed record TopologyResponse(
    int Root,
    int Sentinel,
    int NamespaceStart,
    int NamespaceEnd,
    int AvailablePorts);

public sealed record PortsResponse(
    int NamespaceStart,
    int NamespaceEnd,
    IReadOnlyList<Monolith.Shared.PortMetadata> Services,
    IReadOnlyList<Monolith.Shared.FeatureNamespaceDefinition> FeatureNamespaces,
    int AvailableFeatureIdentities);

public sealed record PortResponse(int Port, string Name, string Status);

public sealed record FeatureResponse(
    int Port,
    string FeatureId,
    string Category,
    string Name,
    string Description,
    string Status);

public sealed record FeatureAssignmentRequest(string Name, string Description);

public sealed record HomeResponse(
    string Service,
    int Port,
    string Purpose,
    IReadOnlyList<string> Endpoints);
