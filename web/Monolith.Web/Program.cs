using System.Security.Claims;
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
builder.Services.AddSingleton<TemplateCatalog>();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<WorkspaceStore>();
builder.Services.AddSingleton<MediaLibrary>();
builder.Services.AddHostedService<PipelineWorker>();
var databasePath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "monolith.db");
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
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
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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

app.MapGet("/api/media", (MediaLibrary media) => Results.Ok(media.GetFiles()))
    .RequireAuthorization();

app.MapPost("/api/media", async (IFormFile file, MediaLibrary media, CancellationToken cancellationToken) =>
    Results.Ok(new { name = await media.SaveAsync(file, cancellationToken) }))
    .RequireAuthorization()
    .DisableAntiforgery();

app.MapPost("/api/auth/login", async (HttpContext context, AccountService accounts) =>
{
    var form = await context.Request.ReadFormAsync();
    var userName = form["userName"].ToString();
    var password = form["password"].ToString();

    if (!accounts.Validate(userName, password))
        return Results.LocalRedirect("/login?error=1");

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, AccountService.UserName),
        new Claim(ClaimTypes.Role, "Producer")
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    return Results.LocalRedirect("/");
}).DisableAntiforgery();

app.MapPost("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/login");
}).DisableAntiforgery();

app.Run();
