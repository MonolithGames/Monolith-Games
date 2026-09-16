namespace Monolith.Web.Models;

public sealed record GameTemplate(
    string Slug,
    string Name,
    string Category,
    string Description,
    string Status,
    string Accent,
    string[] Pipeline);
