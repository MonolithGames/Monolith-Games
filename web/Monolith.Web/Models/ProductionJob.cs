namespace Monolith.Web.Models;

public sealed record ProductionJob(
    Guid Id,
    Guid? ProjectId,
    string TemplateSlug,
    string TemplateName,
    string Status,
    int Progress,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateJobRequest(string TemplateSlug, Guid? ProjectId = null);
