using System.Text.Json;

namespace Monolith.Services;

public sealed class KrakenReadOnlyClient(HttpClient httpClient)
{
    private const string BaseUrl = "https://api.kraken.com";

    public async Task<JsonElement?> GetAssetsAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"{BaseUrl}/0/public/Assets", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.Clone();
    }

    public async Task<JsonElement?> GetAssetPairsAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"{BaseUrl}/0/public/AssetPairs", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.Clone();
    }

    public async Task<JsonElement?> GetTickerAsync(string pair, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"{BaseUrl}/0/public/Ticker?pair={pair}", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.Clone();
    }
}
