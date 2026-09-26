namespace Monolith.Services;

using System;
using System.Collections.Generic;
using Monolith.Shared;

public sealed class AuditRecord
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Actor { get; set; } = "SystemRoot";
    public string Action { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
}

public sealed class EnterpriseAuditService
{
    private readonly List<AuditRecord> _auditLog = new();

    public void RecordEvent(string actor, string action, bool status)
    {
        var record = new AuditRecord
        {
            Actor = actor,
            Action = action,
            Status = status
        };
        _auditLog.Add(record);
        EnterpriseLogger.LogStructured("AUDIT", "AuditService", $"Action '{action}' executed by {actor} with status {status}");
    }

    public IReadOnlyList<AuditRecord> GetAuditTrail() => _auditLog.AsReadOnly();
}
