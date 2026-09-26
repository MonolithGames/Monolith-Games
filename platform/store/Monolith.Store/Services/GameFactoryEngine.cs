namespace Monolith.Store.Services;

public sealed record GeneratedGameSpec(int GameId, string Title, string Theme, string PackageName, string BuildStatus);

public sealed class GameFactoryEngine
{
    private static readonly string[] Prefixes = ["Neon", "Quantum", "Cyber", "Void", "Aether", "Solar", "Plasma", "Hyper", "Nova", "Stellar"];
    private static readonly string[] Suffixes = ["Drift", "Pulse", "Nexus", "Strike", "Vortex", "Matrix", "Horizon", "Echo", "Grid", "Ascent"];

    public List<GeneratedGameSpec> GenerateBatch(int startIndex, int count)
    {
        var batch = new List<GeneratedGameSpec>();
        Random rng = new Random(42 + startIndex);

        for (int i = 0; i < count; i++)
        {
            int id = startIndex + i;
            string prefix = Prefixes[rng.Next(Prefixes.Length)];
            string suffix = Suffixes[rng.Next(Suffixes.Length)];
            string title = $"{prefix} {suffix} {id}";
            string pkg = $"com.monolith.factory.game{id:D4}";

            batch.Add(new GeneratedGameSpec(id, title, "Sci-Fi Arcade", pkg, "Synthesized & Ready"));
        }

        return batch;
    }
}
