namespace Monolith.Tools;

using System;
using System.IO;
using System.Text.Json;

public sealed class BillingSkuGenerator
{
    public static void ExportGooglePlayIapJson(string outputPath)
    {
        var skus = new[]
        {
            new { sku = "remove_ads", type = "inapp", price = "$0.99", title = "Remove Advertisements", description = "Permanently disable all interstitial and banner advertisements." },
            new { sku = "coin_pack_small", type = "inapp", price = "$0.99", title = "500 In-Game Coins", description = "Instantly receive 500 game coins." },
            new { sku = "coin_pack_large", type = "inapp", price = "$4.99", title = "3,000 In-Game Coins", description = "Instantly receive 3,000 game coins + VIP badge." },
            new { sku = "executive_pass", type = "inapp", price = "$19.99", title = "Enterprise Executive Pass", description = "Unlock all enterprise simulation features, cluster scaling, and audit tools." }
        };

        string json = JsonSerializer.Serialize(skus, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outputPath, json);
        Console.WriteLine($"[BILLING SKU GENERATOR] Exported Google Play Console IAP JSON to: {outputPath}");
    }
}
