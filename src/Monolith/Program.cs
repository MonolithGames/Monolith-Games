using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Monolith.Analytics;
using Monolith.Cache;
using Monolith.Components;
using Monolith.Data;
using Monolith.Events;
using Monolith.Integration;
using Monolith.Models;
using Monolith.Services;
using Monolith.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddProblemDetails();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
});
var dataPath = builder.Configuration["MONOLITH_DATA_PATH"] ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataPath, "DataProtection-Keys")))
    .SetApplicationName("Monolith");
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("uploads", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddSingleton<TemplateCatalog>();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<WorkspaceStore>();
builder.Services.AddSingleton<MediaLibrary>();
builder.Services.AddSingleton<CoinbaseSettingsStore>();
builder.Services.AddSingleton<AuditService>();
builder.Services.AddSingleton<IDataProvider, DataService>();
builder.Services.AddSingleton<MemoryCache>();
builder.Services.AddSingleton<ICacheProvider, CacheService>();
builder.Services.AddSingleton<IEventBus, EventBus>();
builder.Services.AddSingleton(sp => new IntegrationOptions
{
    Enabled = bool.TryParse(sp.GetRequiredService<IConfiguration>()["Integration:Enabled"], out var enabled) && enabled,
    ProviderName = sp.GetRequiredService<IConfiguration>()["Integration:ProviderName"] ?? "development-weather",
    BaseUrl = sp.GetRequiredService<IConfiguration>()["Integration:BaseUrl"],
    UserAgent = sp.GetRequiredService<IConfiguration>()["Integration:UserAgent"] ?? "Monolith/1.0",
    Timeout = TimeSpan.FromSeconds(double.TryParse(sp.GetRequiredService<IConfiguration>()["Integration:TimeoutSeconds"], out var timeout) ? timeout : 15)
});
builder.Services.AddHttpClient<IIntegrationClient, IntegrationService>();
builder.Services.AddSingleton<IAnalyticsProvider, AnalyticsService>();
builder.Services.AddSingleton<TradingLimits>();
builder.Services.AddSingleton<TradingPolicy>();
builder.Services.AddSingleton<PaperTradingService>();
builder.Services.AddSingleton<CoinbaseTradingClient>();
builder.Services.AddHttpClient("coinbase", client => client.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddSingleton<CoinbaseReadOnlyClient>();
builder.Services.AddSingleton<CoinbaseFinancialDataService>();
builder.Services.AddHostedService<PipelineWorker>();
var databasePath = Path.Combine(dataPath, "monolith.db");
builder.Services.AddDbContextFactory<MonolithDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "monolith.auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<MonolithDbContext>();
    database.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHealthChecks("/health");

app.MapGet("/api/data/status", (IDataProvider provider) => Results.Ok(ToFeatureStatus(65001, "Data", provider.GetStatus(), new Dictionary<string, object?> { ["records"] = provider.Count })));
app.MapGet("/api/cache/status", (ICacheProvider provider) => Results.Ok(ToFeatureStatus(65002, "Cache", provider.GetStatus(), new Dictionary<string, object?> { ["statistics"] = provider.GetStatistics() })));
app.MapGet("/api/events/status", (IEventBus eventBus) => Results.Ok(ToFeatureStatus(65003, "Events", eventBus.GetStatus(), new Dictionary<string, object?> { ["statistics"] = eventBus.GetStatistics() })));
app.MapGet("/api/integration/status", (IIntegrationClient client) => Results.Ok(ToFeatureStatus(65004, "Integration", client.GetStatus(), new Dictionary<string, object?> { ["provider"] = client.GetProviderStatus() })));
app.MapGet("/api/analytics/status", (IAnalyticsProvider provider) => Results.Ok(ToFeatureStatus(65005, "Analytics", provider.GetStatus(), new Dictionary<string, object?> { ["summary"] = provider.GetSummary() })));

app.MapPost("/api/data/records", async (DataWriteRequest request, IDataProvider provider, CancellationToken cancellationToken) =>
{
    try { return Results.Created($"/api/data/records/{request.Key}", await provider.UpsertAsync(request.Key, request.Value, cancellationToken)); }
    catch (ArgumentException exception) { return Results.Problem(exception.Message, statusCode: 400); }
});
app.MapGet("/api/data/records/{key}", async (string key, IDataProvider provider, CancellationToken cancellationToken) =>
    await provider.GetAsync(key, cancellationToken) is { } item ? Results.Ok(item) : Results.NotFound());
app.MapGet("/api/data/records", async (int offset, int limit, IDataProvider provider, CancellationToken cancellationToken) =>
{
    try { return Results.Ok(await provider.ListAsync(offset, limit == 0 ? 50 : limit, cancellationToken)); }
    catch (ArgumentOutOfRangeException exception) { return Results.Problem(exception.Message, statusCode: 400); }
});
app.MapDelete("/api/data/records/{key}", async (string key, IDataProvider provider, CancellationToken cancellationToken) =>
    await provider.DeleteAsync(key, cancellationToken) ? Results.NoContent() : Results.NotFound());

app.MapPut("/api/cache/entries/{key}", async (string key, CacheWriteRequest request, ICacheProvider provider, CancellationToken cancellationToken) =>
{
    try { await provider.SetJsonAsync(key, request.Value, request.TtlSeconds is null ? null : TimeSpan.FromSeconds(request.TtlSeconds.Value), cancellationToken); return Results.NoContent(); }
    catch (ArgumentOutOfRangeException exception) { return Results.Problem(exception.Message, statusCode: 400); }
    catch (ArgumentException exception) { return Results.Problem(exception.Message, statusCode: 400); }
});
app.MapGet("/api/cache/entries/{key}", async (string key, ICacheProvider provider, CancellationToken cancellationToken) =>
    await provider.GetJsonAsync(key, cancellationToken) is { } value ? Results.Ok(value) : Results.NotFound());
app.MapDelete("/api/cache/entries/{key}", async (string key, ICacheProvider provider, CancellationToken cancellationToken) =>
    await provider.RemoveAsync(key, cancellationToken) ? Results.NoContent() : Results.NotFound());
app.MapPost("/api/cache/maintenance/purge-expired", (ICacheProvider provider) => Results.Ok(new { removed = provider.PurgeExpired() }));

app.MapPost("/api/events", async (EventPublishRequest request, IEventBus bus, CancellationToken cancellationToken) =>
{
    var envelope = new EventEnvelope(Guid.NewGuid(), request.EventType, request.CorrelationId ?? Guid.NewGuid().ToString("N"), request.Source ?? "Monolith", DateTimeOffset.UtcNow, request.Payload);
    await bus.PublishAsync(envelope, cancellationToken);
    return Results.Accepted("/api/events/recent", envelope);
});
app.MapGet("/api/events/recent", (int limit, IEventBus bus) => Results.Ok(bus.GetRecent(limit == 0 ? 50 : limit)));

app.MapPost("/api/integration/weather/refresh", async (IIntegrationClient integration, ICacheProvider cache, IDataProvider data, IEventBus events, IAnalyticsProvider analytics, CancellationToken cancellationToken) =>
    Results.Ok(await integration.RefreshWeatherAsync(cache, data, events, analytics, cancellationToken)));
app.MapGet("/api/integration/providers", (IIntegrationClient integration) => Results.Ok(new[] { integration.GetProviderStatus() }));

app.MapGet("/api/analytics/summary", (IAnalyticsProvider analytics) => Results.Ok(analytics.GetSummary()));
app.MapGet("/api/analytics/features", (IAnalyticsProvider analytics) => Results.Ok(analytics.GetSummary().Counters.Where(pair => pair.Key.Contains('.', StringComparison.Ordinal)).ToDictionary()));
app.MapGet("/api/features", (IDataProvider data, ICacheProvider cache, IEventBus events, IIntegrationClient integration, IAnalyticsProvider analytics) => Results.Ok(new[]
{
    ToFeatureStatus(65001, "Data", data.GetStatus(), new Dictionary<string, object?> { ["records"] = data.Count }),
    ToFeatureStatus(65002, "Cache", cache.GetStatus(), new Dictionary<string, object?> { ["entries"] = cache.GetStatistics().Entries }),
    ToFeatureStatus(65003, "Events", events.GetStatus(), new Dictionary<string, object?> { ["recent"] = events.GetStatistics().RecentCount }),
    ToFeatureStatus(65004, "Integration", integration.GetStatus(), new Dictionary<string, object?> { ["provider"] = integration.GetProviderStatus() }),
    ToFeatureStatus(65005, "Analytics", analytics.GetStatus(), new Dictionary<string, object?> { ["summary"] = analytics.GetSummary() })
}));

app.MapGet("/api/templates", (TemplateCatalog catalog) =>
    Results.Ok(catalog.GetAll()))
    .WithName("GetTemplates");

app.MapGet("/api/templates/{slug}", (string slug, TemplateCatalog catalog) =>
    catalog.GetBySlug(slug) is { } template
        ? Results.Ok(template)
        : Results.NotFound());

app.MapGet("/api/jobs", (WorkspaceStore store) => Results.Ok(store.GetJobs()))
    .RequireAuthorization();

app.MapGet("/api/projects", (WorkspaceStore store) => Results.Ok(store.GetProjects()))
    .RequireAuthorization();

app.MapGet("/api/projects/{id:guid}", (Guid id, WorkspaceStore store) =>
    store.GetProject(id) is { } project ? Results.Ok(project) : Results.NotFound())
    .RequireAuthorization();

app.MapGet("/api/projects/{id:guid}/jobs", (Guid id, WorkspaceStore store) =>
    Results.Ok(store.GetProjectJobs(id)))
    .RequireAuthorization();

app.MapPost("/api/projects", (CreateProjectRequest request, WorkspaceStore store) =>
    store.CreateProject(request.Name, request.TemplateSlug, request.Description) is { } project
        ? Results.Created($"/api/projects/{project.Id}", project)
        : Results.BadRequest(new { error = "A valid name and template are required" }))
    .RequireAuthorization();

app.MapPost("/api/jobs", (CreateJobRequest request, WorkspaceStore store) =>
    store.CreateJob(request.TemplateSlug, request.ProjectId) is { } job
        ? Results.Created($"/api/jobs/{job.Id}", job)
        : Results.NotFound(new { error = "Unknown template" }))
    .RequireAuthorization();

app.MapPost("/api/jobs/{id:guid}/advance", (Guid id, WorkspaceStore store) =>
    store.AdvanceJob(id) is { } job
        ? Results.Ok(job)
        : Results.NotFound())
    .RequireAuthorization();

app.MapGet("/api/jobs/{id:guid}/logs", (Guid id, WorkspaceStore store) =>
    Results.Ok(store.GetLogs(id)))
    .RequireAuthorization();

app.MapPost("/api/jobs/{id:guid}/approve", (Guid id, WorkspaceStore store) =>
    store.ApproveJob(id) is { } job
        ? Results.Ok(job)
        : Results.NotFound())
    .RequireAuthorization();

app.MapPost("/api/jobs/{id:guid}/cancel", (Guid id, WorkspaceStore store) =>
    store.CancelJob(id) is { } job ? Results.Ok(job) : Results.NotFound())
    .RequireAuthorization();

app.MapPost("/api/jobs/{id:guid}/retry", (Guid id, WorkspaceStore store) =>
    store.RetryJob(id) is { } job ? Results.Ok(job) : Results.NotFound())
    .RequireAuthorization();

app.MapGet("/api/adapters", (IConfiguration configuration) => Results.Ok(new
{
    azure = !string.IsNullOrWhiteSpace(configuration["MONOLITH_AZURE_COMMAND"]),
    maya = !string.IsNullOrWhiteSpace(configuration["MONOLITH_MAYA_COMMAND"]),
    unity = !string.IsNullOrWhiteSpace(configuration["MONOLITH_UNITY_COMMAND"]),
    media = !string.IsNullOrWhiteSpace(configuration["MONOLITH_MEDIA_COMMAND"]),
    publishing = !string.IsNullOrWhiteSpace(configuration["MONOLITH_PUBLISH_COMMAND"])
})).RequireAuthorization();

app.MapGet("/api/settings/coinbase", (CoinbaseSettingsStore settings) =>
    Results.Ok(settings.GetStatus())).RequireAuthorization();

app.MapGet("/api/trading/status", (TradingPolicy policy) =>
    Results.Ok(policy.GetStatus())).RequireAuthorization();

app.MapGet("/api/audit", (AuditService audit) =>
    Results.Ok(audit.GetRecent())).RequireAuthorization();

app.MapGet("/api/paper/status", (PaperTradingService paper) =>
    Results.Ok(paper.GetStatus())).RequireAuthorization();

app.MapGet("/api/paper/orders", (PaperTradingService paper) =>
    Results.Ok(paper.GetOrders())).RequireAuthorization();

app.MapPost("/api/paper/orders", (HttpContext context, PaperOrderRequest request, PaperTradingService paper) =>
{
    try
    {
        var actor = context.User.Identity?.Name ?? "unknown";
        return Results.Created("/api/paper/orders", paper.PlaceOrder(actor, request));
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
}).RequireAuthorization();

app.MapPost("/api/live/orders", async (HttpContext context, LiveOrderRequest request, CoinbaseTradingClient trading, CancellationToken cancellationToken) =>
{
    try
    {
        var actor = context.User.Identity?.Name ?? "unknown";
        return (IResult)Results.Created("/api/live/orders", await trading.PlaceOrderAsync(actor, request, cancellationToken));
    }
    catch (InvalidOperationException exception)
    {
        return (IResult)Results.BadRequest(new { error = exception.Message });
    }
    catch (HttpRequestException exception)
    {
        return (IResult)Results.Problem(exception.Message, statusCode: StatusCodes.Status502BadGateway);
    }
}).RequireAuthorization();

app.MapGet("/api/coinbase/products", async (CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetProductsAsync(cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/products/{productId}", async (string productId, CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetProductAsync(productId, cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/products/{productId}/candles", async (string productId, long start, long end, string granularity, CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetCandlesAsync(productId, start, end, granularity, cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/best-bid-ask", async (string productIds, CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetBestBidAskAsync(productIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/products/{productId}/ticker", async (string productId, int limit, CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetMarketTradesAsync(productId, limit, cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/product-book/{productId}", async (string productId, int limit, CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetProductBookAsync(productId, limit, cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/orders", async (string? productId, string? orderStatus, int limit, CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetOrdersAsync(productId, orderStatus, limit, cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/orders/{orderId}", async (string orderId, CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetOrderAsync(orderId, cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/fills", async (string? productId, string? orderId, int limit, CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetFillsAsync(productId, orderId, limit, cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/transaction-summary", async (CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetTransactionSummaryAsync(cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/accounts", async (CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetAccountsAsync(cancellationToken))).RequireAuthorization();

app.MapGet("/api/coinbase/portfolios", async (CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetPortfoliosAsync(cancellationToken))).RequireAuthorization();

app.MapGet("/api/media", (MediaLibrary media) => Results.Ok(media.GetFiles()))
    .RequireAuthorization();

app.MapPost("/api/media", async (IFormFile file, MediaLibrary media, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(new { name = await media.SaveAsync(file, cancellationToken) });
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
})
    .RequireAuthorization()
    .RequireRateLimiting("uploads")
    .DisableAntiforgery();

app.MapPost("/api/auth/login", async (HttpContext context, AccountService accounts, AuditService audit) =>
{
    var form = await context.Request.ReadFormAsync();
    var userName = form["userName"].ToString();
    var password = form["password"].ToString();

    if (!accounts.Validate(userName, password))
    {
        audit.Record(userName.Length == 0 ? "anonymous" : userName, "auth.login", "session", false);
        return Results.LocalRedirect("/login?error=1");
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, AccountService.UserName),
        new Claim(ClaimTypes.Role, "Producer")
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    audit.Record(AccountService.UserName, "auth.login", "session", true);
    return Results.LocalRedirect("/");
}).RequireRateLimiting("auth").DisableAntiforgery();

app.MapPost("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/login");
}).DisableAntiforgery();

app.Run();

static FeatureStatus ToFeatureStatus(int identity, string name, PlatformComponentStatus status, IReadOnlyDictionary<string, object?> details) =>
    new(identity, name, status.Status, DateTimeOffset.UtcNow, details);

public sealed record DataWriteRequest(string Key, JsonElement Value);
public sealed record CacheWriteRequest(JsonElement Value, int? TtlSeconds);
public sealed record EventPublishRequest(string EventType, string? CorrelationId, string? Source, JsonElement Payload);
