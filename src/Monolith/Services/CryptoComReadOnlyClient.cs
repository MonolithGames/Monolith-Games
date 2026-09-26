using System.Text.Json;

namespace Monolith.Services;

public sealed class CryptoComReadOnlyClient(HttpClient httpClient)
{
    private const string BaseUrl = "https://api.crypto.com/v2";

    public async Task<JsonElement?> GetInstrumentsAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"{BaseUrl}/public/get-instruments", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.Clone();
    }
}
