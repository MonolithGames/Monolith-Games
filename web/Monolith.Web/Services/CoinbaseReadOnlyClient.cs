using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Monolith.Web.Services;

public sealed class CoinbaseReadOnlyClient(
    IHttpClientFactory httpClientFactory,
    CoinbaseSettingsStore settings,
    AuditService audit,
    ILogger<CoinbaseReadOnlyClient> logger)
{
    private const string BaseUrl = "https://api.coinbase.com";

    public Task<JsonElement?> GetProductsAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/v3/brokerage/products", cancellationToken);

    public Task<JsonElement?> GetAccountsAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/v3/brokerage/accounts", cancellationToken);

    public Task<JsonElement?> GetPortfoliosAsync(CancellationToken cancellationToken) =>
        GetAsync("/api/v3/brokerage/portfolios", cancellationToken);

    private async Task<JsonElement?> GetAsync(string path, CancellationToken cancellationToken)
    {
        var material = settings.GetCredentialMaterial();
        if (material is null)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + path);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken(material.Value.ApiKeyName, material.Value.PrivateKey, "GET", path));
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

        if (response is null)
            throw new HttpRequestException("Coinbase did not return a response.");

        using (response)
        {
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Coinbase read-only request {Path} failed with {Status}: {Body}", path, response.StatusCode, body);
            audit.Record("system", "coinbase.read", path, false);
            throw new HttpRequestException($"Coinbase returned {(int)response.StatusCode} for {path}.");
        }

        audit.Record("system", "coinbase.read", path, true);
        return System.Text.Json.JsonSerializer.Deserialize<JsonElement>(body);
        }
    }

    private static string CreateToken(string keyName, string privateKey, string method, string path)
    {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privateKey);
        var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyName };
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);
        var now = DateTimeOffset.UtcNow;
        var token = new JwtSecurityToken(
            issuer: "cdp",
            claims: new[]
            {
                new System.Security.Claims.Claim("sub", keyName),
                new System.Security.Claims.Claim("uri", $"{method} api.coinbase.com{path}")
            },
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(2).UtcDateTime,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}