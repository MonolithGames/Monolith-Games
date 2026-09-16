using Monolith.Web.Components;
using Monolith.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<TemplateCatalog>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
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

app.Run();
