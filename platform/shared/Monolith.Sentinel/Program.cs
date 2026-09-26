using System.Net.Http.Json;
using Monolith.Shared;
using Monolith.Shared.Features;
using Monolith.Sentinel.Models;
using Monolith.Sentinel.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["urls"] ?? "http://0.0.0.0:65535");
builder.Services.AddSingleton<PlatformState>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<PlatformRegistry>();
builder.Services.AddSingleton<FeatureRegistry>();
builder.Services.AddSingleton(sp => new SentinelOptions
{
    MonolithBaseUrl = sp.GetRequiredService<IConfiguration>()["Sentinel:MonolithBaseUrl"] ?? "http://localhost:65000",
    DependencyTimeoutSeconds = int.TryParse(sp.GetRequiredService<IConfiguration>()["Sentinel:DependencyTimeoutSeconds"], out var timeout) ? timeout : 3
});
builder.Services.AddHttpClient("monolith", (sp, client) =>
{
    var options = sp.GetRequiredService<SentinelOptions>();
    client.BaseAddress = new Uri(options.MonolithBaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.DependencyTimeoutSeconds, 1, 10));
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.MapGet("/health", () =>
{
    var response = new HealthResponse("healthy", DateTimeOffset.UtcNow);
    return Results.Json(response);
});

app.MapGet("/health/live", () =>
    Results.Json(new HealthResponse("healthy", DateTimeOffset.UtcNow)));

app.MapGet("/health/ready", (PlatformRegistry registry) =>
{
    var status = registry.GetPort(PortRegistry.Monolith) is not null &&
        registry.GetPort(PortRegistry.Sentinel) is not null
        ? "ready"
        : "not_ready";
    return Results.Json(new HealthResponse(status, DateTimeOffset.UtcNow));
});

app.MapGet("/status", (PlatformState platformState, IWebHostEnvironment environment) =>
{
    var response = new StatusResponse(
        Service: PlatformState.ServiceName,
        Status: "healthy",
        Version: PlatformState.Version,
        Environment: environment.EnvironmentName,
        MachineName: Environment.MachineName,
        UptimeSeconds: platformState.UptimeSeconds);

    return Results.Json(response);
});

app.MapGet("/version", () =>
{
    var response = new VersionResponse(PlatformState.ServiceName, PlatformState.Version);
    return Results.Json(response);
});

app.MapGet("/time", () =>
{
    var response = new TimeResponse(DateTimeOffset.UtcNow);
    return Results.Json(response);
});

app.MapGet("/components", async (IHttpClientFactory clients, CancellationToken cancellationToken) =>
{
    var components = new List<ComponentInfo>
    {
        new("Monolith", "available", "http://localhost:65000"),
        new("Sentinel", "available", "/health")
    };

    try
    {
        var client = clients.CreateClient("monolith");
        var features = await client.GetFromJsonAsync<Monolith.Shared.FeatureStatus[]>("/api/features", cancellationToken);
        if (features is not null)
        {
            components.AddRange(features.Select(feature =>
                new ComponentInfo(feature.Name, feature.State, $"/api/{feature.Name.ToLowerInvariant()}/status")));
        }
    }
    catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or NotSupportedException or System.Text.Json.JsonException)
    {
        components.Add(new ComponentInfo("Monolith.Modules", "degraded", "/api/features", "Monolith feature discovery is unavailable."));
    }

    var response = new ComponentsResponse(components.ToArray());

    return Results.Json(response);
});

app.MapGet("/topology", (PlatformRegistry registry) =>
{
    var response = new TopologyResponse(
        Root: registry.GetPort(PortRegistry.Monolith)!.Port,
        Sentinel: registry.GetPort(PortRegistry.Sentinel)!.Port,
        NamespaceStart: registry.NamespaceStart,
        NamespaceEnd: registry.NamespaceEnd,
        AvailablePorts: registry.AvailablePortCount);

    return Results.Json(response);
});

app.MapGet("/ports", (PlatformRegistry registry) =>
{
    var response = new PortsResponse(
        registry.NamespaceStart,
        registry.NamespaceEnd,
        registry.Ports.Select(port => registry.GetPort(port.Port)!).ToArray(),
        registry.FeatureNamespaces,
        registry.AvailableFeatureIdentities.Count);

    return Results.Json(response);
});

app.MapGet("/port/{number:int}", (int number, PlatformRegistry registry) =>
{
    var port = registry.GetPort(number);
    return port is null
        ? Results.NotFound()
        : Results.Json(new PortResponse(port.Port, port.Name, port.Status));
});

app.MapGet("/features", (FeatureRegistry registry) =>
    Results.Json(registry.GetAll().Select(ToFeatureResponse).ToArray()));

app.MapGet("/features/{port:int}", (int port, FeatureRegistry registry) =>
{
    var feature = registry.GetByPort(port);
    return feature is null ? Results.NotFound() : Results.Json(ToFeatureResponse(feature));
});

app.MapGet("/features/category/{category}", (string category, FeatureRegistry registry) =>
{
    return Enum.TryParse<FeatureCategory>(category, true, out var parsedCategory)
        ? Results.Json(registry.GetByCategory(parsedCategory).Select(ToFeatureResponse).ToArray())
        : Results.BadRequest(new { error = $"Unknown feature category '{category}'." });
});

app.MapGet("/features/status/{status}", (string status, FeatureRegistry registry) =>
{
    return Enum.TryParse<Monolith.Shared.Features.FeatureStatus>(status, true, out var parsedStatus)
        ? Results.Json(registry.GetByStatus(parsedStatus).Select(ToFeatureResponse).ToArray())
        : Results.BadRequest(new { error = $"Unknown feature status '{status}'." });
});

app.MapPost("/features/{port:int}/assign", (
    int port,
    FeatureAssignmentRequest request,
    FeatureRegistry registry,
    IWebHostEnvironment environment) =>
{
    if (!environment.IsDevelopment())
    {
        return Results.NotFound();
    }

    try
    {
        return Results.Ok(ToFeatureResponse(registry.Assign(port, request.Name, request.Description)));
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { error = exception.Message });
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new { error = exception.Message });
    }
});

app.MapGet("/", () =>
{
    var response = new HomeResponse(
        Service: PlatformState.ServiceName,
        Port: 65535,
        Purpose: "Platform health and diagnostics",
        Endpoints: new[]
        {
            "/health",
            "/status",
            "/version",
            "/time",
            "/components",
            "/topology",
            "/ports",
            "/port/{number}"
        });

    return Results.Json(response);
});

app.Run();

static FeatureResponse ToFeatureResponse(FeatureMetadata feature) =>
    new(
        feature.PortNumber,
        feature.FeatureId,
        feature.Category.ToString(),
        feature.Name,
        feature.Description,
        feature.Status.ToString());
