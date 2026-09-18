using System.Globalization;
using System.Text.Json;
using Monolith.Web.Models;

namespace Monolith.Web.Services;

public sealed class CoinbaseFinancialDataService(CoinbaseReadOnlyClient client)
{
    public async Task<FinancialSnapshot> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            var accounts = await client.GetAccountsAsync(cancellationToken);
            var portfolios = await client.GetPortfoliosAsync(cancellationToken);
            var balances = ParseBalances(accounts);
            var total = FindDecimal(portfolios, "total_balance") ?? FindDecimal(portfolios, "total_value");
            return new(balances, total, DateTime.UtcNow, false, null);
        }
        catch (Exception exception)
        {
            return new([], null, DateTime.UtcNow, true, exception.Message);
        }
    }

    private static IReadOnlyList<AssetBalance> ParseBalances(JsonElement? response)
    {
        if (response is not { ValueKind: JsonValueKind.Object } root || !root.TryGetProperty("accounts", out var accounts) || accounts.ValueKind != JsonValueKind.Array)
            return [];

        var balances = new List<AssetBalance>();
        foreach (var account in accounts.EnumerateArray())
        {
            var asset = GetString(account, "currency") ?? GetString(account, "asset");
            if (string.IsNullOrWhiteSpace(asset))
                continue;
            var available = FindDecimal(account, "available_balance") ?? FindDecimal(account, "available");
            var held = FindDecimal(account, "hold") ?? FindDecimal(account, "held");
            if (available is null && held is null)
                continue;
            balances.Add(new(asset, available ?? 0m, held ?? 0m, asset));
        }
        return balances.OrderByDescending(item => item.Available + item.Held).ToArray();
    }

    private static string? GetString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static decimal? FindDecimal(JsonElement? element, string property)
    {
        if (element is not { ValueKind: JsonValueKind.Object } objectElement || !objectElement.TryGetProperty(property, out var value))
            return null;
        if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("value", out var nested))
            value = nested;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
            return number;
        if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var text))
            return text;
        return null;
    }
}
