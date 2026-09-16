using Microsoft.EntityFrameworkCore;

namespace Monolith.Web.Data;

public sealed class MonolithDbContext(DbContextOptions<MonolithDbContext> options) : DbContext(options)
{
    public DbSet<JobEntity> Jobs => Set<JobEntity>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<JobLogEntity> JobLogs => Set<JobLogEntity>();
}

public sealed class ProjectEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string TemplateSlug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class JobEntity
{
    public Guid Id { get; set; }
    public Guid? ProjectId { get; set; }
    public string TemplateSlug { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class JobLogEntity
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
