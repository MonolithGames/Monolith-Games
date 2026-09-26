using System.Collections.ObjectModel;

namespace Monolith.Shared;

public static class FeatureIdentityRegistry
{
    public static IReadOnlyDictionary<int, string> Identities { get; } =
        new ReadOnlyDictionary<int, string>(new Dictionary<int, string>
        {
            [65000] = "Monolith",
            [65001] = "Data",
            [65002] = "Cache",
            [65003] = "Events",
            [65004] = "Integration",
            [65005] = "Analytics",
            [65535] = "Monolith.Sentinel"
        });
}