using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Monolith.Web.Components;
using Monolith.Web.Data;
using Monolith.Web.Models;
using Monolith.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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
builder.Services.AddSingleton<TradingPolicy>();
builder.Services.AddSingleton<PaperTradingService>();
builder.Services.AddHttpClient("coinbase", client => client.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddSingleton<CoinbaseReadOnlyClient>();
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

app.MapGet("/api/coinbase/products", async (CoinbaseReadOnlyClient coinbase, CancellationToken cancellationToken) =>
    Results.Ok(await coinbase.GetProductsAsync(cancellationToken))).RequireAuthorization();

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
