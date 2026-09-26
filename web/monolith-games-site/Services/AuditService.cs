using Microsoft.EntityFrameworkCore;
using Monolith.Data;
using Monolith.Models;

namespace Monolith.Services;

public sealed class AuditService(IDbContextFactory<MonolithDbContext> dbFactory)
{
    public void Record(string actor, string action, string resource, bool success)
    {
        using var db = dbFactory.CreateDbContext();
        db.AuditEvents.Add(new AuditEventEntity
        {
            Id = Guid.NewGuid(), Actor = actor, Action = action, Resource = resource,
            Success = success, CreatedAtUtc = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    public IReadOnlyList<AuditEvent> GetRecent(int limit = 100)
    {
        using var db = dbFactory.CreateDbContext();
        return db.AuditEvents.AsNoTracking().OrderByDescending(item => item.CreatedAtUtc).Take(limit)
            .Select(item => new AuditEvent(item.Id, item.Actor, item.Action, item.Resource, item.Success, item.CreatedAtUtc)).ToArray();
    }
}