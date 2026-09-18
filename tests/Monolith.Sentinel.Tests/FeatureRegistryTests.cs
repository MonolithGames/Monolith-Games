using Monolith.Shared.Features;

FeatureRegistryTests.Run();

static class FeatureRegistryTests
{
    public static void Run()
    {
        var registry = new FeatureRegistry();
        Assert(registry.GetAll().Count == 534, "Feature registry did not generate 534 identities.");
        Assert(registry.GetByCategory(FeatureCategory.Data).Count == 89, "Data category size is incorrect.");

        var identity = registry.Assign(65090, "WeatherCache", "Weather caching capability");
        Assert(identity.Status == FeatureStatus.Assigned, "Feature assignment did not update status.");
        Assert(registry.GetByName("WeatherCache")?.PortNumber == 65090, "Feature name lookup failed.");

        Console.WriteLine("Feature registry tests passed.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}