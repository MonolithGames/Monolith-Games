namespace Monolith.Web.Models;

public sealed record Project(
    Guid Id,
    string Name,
    string Slug,
    string TemplateSlug,
    string Description,
    DateTime CreatedAtUtc);

public sealed record ProjectLog(Guid Id, Guid JobId, string Message, DateTime CreatedAtUtc);

public sealed record CreateProjectRequest(string Name, string TemplateSlug, string Description);
