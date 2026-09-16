using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Web.Data;
using Monolith.Web.Services;

namespace Monolith.Web.Tests;

public sealed class PlatformTests
{
    [Fact]
    public void CatalogContainsTheThreeProductionTemplates()
    {
        var templates = new TemplateCatalog().GetAll();

        Assert.Equal(3, templates.Count);
        Assert.Contains(templates, template => template.Slug == "skyline-runner");
        Assert.Contains(templates, template => template.Slug == "neon-kart");
        Assert.Contains(templates, template => template.Slug == "pocket-planet");
    }

    [Fact]
    public void JobsPersistAndAdvanceThroughTheWorkspaceStore()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var services = new ServiceCollection();
        services.AddDbContextFactory<MonolithDbContext>(options =>
            options.UseSqlite(connection));
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbContextFactory<MonolithDbContext>>();
        using var database = factory.CreateDbContext();
        database.Database.EnsureCreated();

        var store = new WorkspaceStore(factory, new TemplateCatalog());
        var job = store.CreateJob("skyline-runner");

        Assert.NotNull(job);
        Assert.Equal("Queued", job!.Status);
        var advanced = store.AdvanceJob(job.Id);

        Assert.Equal(20, advanced!.Progress);
        Assert.Contains(store.GetLogs(job.Id), log => log.Message.Contains("20%"));
    }
}
