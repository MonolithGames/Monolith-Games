using Monolith.Web.Models;

namespace Monolith.Web.Services;

public sealed class TemplateCatalog
{
    private static readonly IReadOnlyList<GameTemplate> Templates =
    [
        new(
            "skyline-runner",
            "Skyline Runner",
            "3D action",
            "A rooftop chase built around quick movement, vertical level design, and precise combat.",
            "Ready for production",
            "#e56b52",
            ["Validate", "Maya assets", "Unity build", "Quality check", "Package"]),
        new(
            "neon-kart",
            "Neon Kart",
            "Multiplayer racing",
            "A bright arcade racer with short competitive sessions and a strong social loop.",
            "Adapter review needed",
            "#33a6a8",
            ["Validate", "Maya assets", "Unity build", "Network QA", "Package"]),
        new(
            "pocket-planet",
            "Pocket Planet",
            "Casual mobile",
            "A friendly collection game designed for quick sessions, readable goals, and mobile touch input.",
            "Ready for production",
            "#d79a3d",
            ["Validate", "Maya assets", "Unity build", "Device QA", "Package"])
    ];

    public IReadOnlyList<GameTemplate> GetAll() => Templates;

    public GameTemplate? GetBySlug(string slug) =>
        Templates.FirstOrDefault(template =>
            string.Equals(template.Slug, slug, StringComparison.OrdinalIgnoreCase));
}
