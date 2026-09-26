namespace Monolith.Store.Services;

public sealed record GeneratedGameSpec(int GameId, string Title, string Theme, string PackageName, string BuildStatus);

public sealed class GameFactoryEngine
{
    private static readonly string[] BaseTitles = [
        "Void Strikers", "Kinetics Zero", "Chronos Breach", "Nebula Veil", "Valkyrie Ascendant",
        "Shattered Horizon", "Phantom Grid", "Solaris Drift", "Abyssal Echo", "Apex Dominion",
        "Island Monolith 3D", "Corp Studio Nexus", "Corp Venture Tactics", "Corp Apex Sim",
        "Corp Quantum Ops", "Corp Cyber Defense", "Corp Global Logistics", "Corp BioTech Genomics",
        "Corp Neural Net AI", "Enterprise Flight Sim", "Enterprise Supply Chain", "Enterprise Market Trader",
        "Enterprise Grid Manager", "Enterprise Medical Triage", "Kids ABC Shapes", "Idle Magistrate",
        "Defense Grid Tactics", "Crystal Match 3", "Neon Rhythm Beat", "Dungeon Roguelike",
        "Aetheria Void", "Quantum Drift", "Cyber Pulse", "Stellar Matrix", "Plasma Nexus",
        "Nova Horizon", "Hyper Strike", "Solar Vortex", "Neon Grid", "Aether Ascent"
    ];

    private static readonly string[] SequelNumerals = ["", " II", " III", " IV"];

    public List<GeneratedGameSpec> GenerateBatch(int startIndex, int count)
    {
        var batch = new List<GeneratedGameSpec>();

        for (int i = 0; i < count; i++)
        {
            int id = startIndex + i;
            string baseTitle = BaseTitles[i % BaseTitles.Length];
            int sequelIndex = (i / BaseTitles.Length) % SequelNumerals.Length;
            string numeral = SequelNumerals[sequelIndex];
            string title = $"{baseTitle}{numeral}";

            string pkgName = $"com.monolith.game.{baseTitle.ToLowerInvariant().Replace(" ", "").Replace(":", "")}{(sequelIndex > 0 ? (sequelIndex + 1).ToString() : "")}";

            batch.Add(new GeneratedGameSpec(id, title, "High-Fidelity Title", pkgName, "Active & Ready"));
        }

        return batch;
    }
}
