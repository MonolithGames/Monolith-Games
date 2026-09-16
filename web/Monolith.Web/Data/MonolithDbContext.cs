using Microsoft.EntityFrameworkCore;

namespace Monolith.Web.Data;

public sealed class MonolithDbContext(DbContextOptions<MonolithDbContext> options) : DbContext(options)
{
    public DbSet<JobEntity> Jobs => Set<JobEntity>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<JobLogEntity> JobLogs => Set<JobLogEntity>();
    public DbSet<CoinbaseCredentialEntity> CoinbaseCredentials => Set<CoinbaseCredentialEntity>();
    public DbSet<AuditEventEntity> AuditEvents => Set<AuditEventEntity>();
    public DbSet<PaperOrderEntity> PaperOrders => Set<PaperOrderEntity>();
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

public sealed class CoinbaseCredentialEntity
{
    public int Id { get; set; }
    public string ApiKeyName { get; set; } = string.Empty;
    public string ProtectedPrivateKey { get; set; } = string.Empty;
    public bool LiveTradingEnabled { get; set; }
    public bool WithdrawalsEnabled { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class AuditEventEntity
{
    public Guid Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class PaperOrderEntity
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal LimitPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
