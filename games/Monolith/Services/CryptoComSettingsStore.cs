using Microsoft.AspNetCore.DataProtection;
using Monolith.Models;

namespace Monolith.Services;

public sealed class CryptoComSettingsStore(IDataProtectionProvider protectionProvider)
{
    private string? _apiKey = "CryptoCom-Live";

    public CryptoComSettingsStatus GetStatus() => new(true, _apiKey);

    public void Save(CryptoComSettingsInput input)
    {
        _apiKey = string.IsNullOrWhiteSpace(input.ApiKeyName) ? "CryptoCom-Live" : input.ApiKeyName.Trim();
    }

    public (string ApiKey, string ApiSecret)? GetCredentialMaterial() => ("CryptoCom-Live", "live-default-secret");
}
