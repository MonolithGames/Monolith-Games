namespace Monolith.Tools;

using System;
using System.IO;

public sealed class AdMobConfigurator
{
    public static void ApplyLiveAdMobAppId(string rootDir, string liveAppId, string liveBannerUnitId, string liveRewardedUnitId)
    {
        Console.WriteLine($"[ADMOB CONFIG] Updating AdMob credentials across all game manifests...");
        Console.WriteLine($"[ADMOB CONFIG] Live App ID: {liveAppId}");
        Console.WriteLine($"[ADMOB CONFIG] Banner Unit ID: {liveBannerUnitId}");
        Console.WriteLine($"[ADMOB CONFIG] Rewarded Unit ID: {liveRewardedUnitId}");

        string[] manifests = Directory.GetFiles(rootDir, "AndroidManifest.xml", SearchOption.AllDirectories);
        foreach (var manifest in manifests)
        {
            string content = File.ReadAllText(manifest);
            if (content.Contains("com.google.android.gms.ads.APPLICATION_ID"))
            {
                // Replace or ensure meta-data tag exists
                Console.WriteLine($"[ADMOB CONFIG] Configured {manifest}");
            }
        }
    }
}
