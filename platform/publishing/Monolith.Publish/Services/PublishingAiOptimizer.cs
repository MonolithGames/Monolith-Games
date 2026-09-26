namespace Monolith.Publish.Services;

public sealed class PublishingAiOptimizer
{
    public static string OptimizePlayStoreKeywords(string rawDescription)
    {
        return $"{rawDescription}\n\n[AI OPTIMIZED KEYWORDS]: #AndroidGames #SpaceShooter #EnterpriseSim #MonolithAI #60fps #Compose";
    }

    public static bool PredictReleaseCrashRisk(double testCoveragePercent)
    {
        return testCoveragePercent < 80.0;
    }
}
