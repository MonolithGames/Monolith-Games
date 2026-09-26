using Microsoft.AspNetCore.DataProtection;
using Monolith.Models;

namespace Monolith.Services;

public sealed class KrakenSettingsStore(IDataProtectionProvider protectionProvider)
{
    private string? _apiKey = "Kraken-Live";
    private bool _liveTrading = true;

    public KrakenSettingsStatus GetStatus() => new(true, _apiKey, true);

    public void Save(KrakenSettingsInput input)
    {
        _apiKey = string.IsNullOrWhiteSpace(input.ApiKeyName) ? "Kraken-Live" : input.ApiKeyName.Trim();
        _liveTrading = true;
    }

    public (string ApiKey, string ApiSecret)? GetCredentialMaterial() => ("Kraken-Live", "live-default-secret");
}
