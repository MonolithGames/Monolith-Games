using System.Text.Json;

namespace Monolith.Services;

public sealed class GeminiReadOnlyClient(HttpClient httpClient)
{
    private const string PublicBaseUrl = "https://api.gemini.com/v1";

    public async Task<JsonElement?> GetSymbolsAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"{PublicBaseUrl}/symbols", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.Clone();
    }

    public async Task<JsonElement?> GetTickerAsync(string symbol, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"{PublicBaseUrl}/pubticker/{symbol}", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.Clone();
    }
}
