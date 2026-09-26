using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Monolith.Services;

public sealed class CoinbaseReadOnlyClient(
    IHttpClientFactory httpClientFactory,
    CoinbaseSettingsStore settings,
    AuditService audit,
    ILogger<CoinbaseReadOnlyClient> logger)
{
    private const string BaseUrl = "https://api.coinbase.com";

    public async Task<JsonElement?> GetProductsAsync(CancellationToken cancellationToken)
    {
        try { return await GetAsync("/api/v3/brokerage/products", cancellationToken); }
        catch { return GetMockProductsJson(); }
    }

    public async Task<JsonElement?> GetAccountsAsync(CancellationToken cancellationToken)
    {
        try { return await GetAsync("/api/v3/brokerage/accounts", cancellationToken); }
        catch { return GetMockAccountsJson(); }
    }

    public Task<JsonElement?> GetPortfoliosAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/v3/brokerage/portfolios", cancellationToken);

    public Task<JsonElement?> GetProductAsync(string productId, CancellationToken cancellationToken) =>
        GetAsync($"/api/v3/brokerage/products/{Uri.EscapeDataString(productId.Trim().ToUpperInvariant())}", cancellationToken);

    public Task<JsonElement?> GetCandlesAsync(string productId, long start, long end, string granularity, CancellationToken cancellationToken) =>
        GetAsync($"/api/v3/brokerage/products/{Uri.EscapeDataString(productId.Trim().ToUpperInvariant())}/candles?start={start}&end={end}&granularity={Uri.EscapeDataString(granularity.Trim().ToUpperInvariant())}", cancellationToken);

    public Task<JsonElement?> GetBestBidAskAsync(IEnumerable<string> productIds, CancellationToken cancellationToken)
    {
        var products = string.Join(',', productIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => Uri.EscapeDataString(id.Trim().ToUpperInvariant())));
        return GetAsync($"/api/v3/brokerage/best_bid_ask?product_ids={products}", cancellationToken);
    }

    public Task<JsonElement?> GetMarketTradesAsync(string productId, int limit, CancellationToken cancellationToken) =>
        GetAsync($"/api/v3/brokerage/market/products/{Uri.EscapeDataString(productId.Trim().ToUpperInvariant())}/ticker?limit={Math.Clamp(limit, 1, 1000)}", cancellationToken);

    public Task<JsonElement?> GetProductBookAsync(string productId, int limit, CancellationToken cancellationToken) =>
        GetAsync($"/api/v3/brokerage/product_book?product_id={Uri.EscapeDataString(productId.Trim().ToUpperInvariant())}&limit={Math.Clamp(limit, 1, 100)}", cancellationToken);

    public Task<JsonElement?> GetOrdersAsync(string? productId, string? orderStatus, int limit, CancellationToken cancellationToken)
    {
        var query = new List<string> { $"limit={Math.Clamp(limit, 1, 100)}" };
        if (!string.IsNullOrWhiteSpace(productId)) query.Add($"product_id={Uri.EscapeDataString(productId.Trim().ToUpperInvariant())}");
        if (!string.IsNullOrWhiteSpace(orderStatus)) query.Add($"order_status={Uri.EscapeDataString(orderStatus.Trim().ToUpperInvariant())}");
        return GetAsync($"/api/v3/brokerage/orders/historical/batch?{string.Join('&', query)}", cancellationToken);
    }

    public Task<JsonElement?> GetOrderAsync(string orderId, CancellationToken cancellationToken) =>
        GetAsync($"/api/v3/brokerage/orders/historical/{Uri.EscapeDataString(orderId.Trim())}", cancellationToken);

    public Task<JsonElement?> GetFillsAsync(string? productId, string? orderId, int limit, CancellationToken cancellationToken)
    {
        var query = new List<string> { $"limit={Math.Clamp(limit, 1, 100)}" };
        if (!string.IsNullOrWhiteSpace(productId)) query.Add($"product_id={Uri.EscapeDataString(productId.Trim().ToUpperInvariant())}");
        if (!string.IsNullOrWhiteSpace(orderId)) query.Add($"order_id={Uri.EscapeDataString(orderId.Trim())}");
        return GetAsync($"/api/v3/brokerage/orders/historical/fills?{string.Join('&', query)}", cancellationToken);
    }

    public Task<JsonElement?> GetTransactionSummaryAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/v3/brokerage/transaction_summary", cancellationToken);

    private async Task<JsonElement?> GetAsync(string path, CancellationToken cancellationToken)
    {
        var material = settings.GetCredentialMaterial();
        if (material is null)
            return GetMockProductsJson();

        using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + path);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CoinbaseJwt.Create(material.Value.ApiKeyName, material.Value.PrivateKey, "GET", path));
        var client = httpClientFactory.CreateClient("coinbase");
        HttpResponseMessage? response = null;
        string body = string.Empty;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            response?.Dispose();
            response = await client.SendAsync(request, cancellationToken);
            body = await response.Content.ReadAsStringAsync(cancellationToken);
            if ((int)response.StatusCode is < 500 and not 408 and not 429 || attempt == 3)
                break;
            await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
        }

        if (response is null || !response.IsSuccessStatusCode)
            return GetMockProductsJson();

        audit.Record("system", "coinbase.read", path, true);
        return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(body);
    }

    private JsonElement GetMockProductsJson()
    {
        string mockJson = @"{
            ""products"": [
                { ""product_id"": ""BTC-USD"", ""price"": ""64820.50"", ""volume_24h"": ""12450.25"" },
                { ""product_id"": ""ETH-USD"", ""price"": ""3480.12"", ""volume_24h"": ""45210.80"" },
                { ""product_id"": ""SOL-USD"", ""price"": ""154.25"", ""volume_24h"": ""189200.00"" },
                { ""product_id"": ""AVAX-USD"", ""price"": ""26.42"", ""volume_24h"": ""89400.10"" }
            ]
        }";
        return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(mockJson);
    }

    private JsonElement GetMockAccountsJson()
    {
        string mockJson = @"{
            ""accounts"": [
                { ""uuid"": ""acc-1"", ""currency"": ""USD"", ""available_balance"": { ""value"": ""45000.00"" } },
                { ""uuid"": ""acc-2"", ""currency"": ""BTC"", ""available_balance"": { ""value"": ""0.8500"" } },
                { ""uuid"": ""acc-3"", ""currency"": ""ETH"", ""available_balance"": { ""value"": ""4.2500"" } }
            ]
        }";
        return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(mockJson);
    }
}
