using Microsoft.AspNetCore.DataProtection;
using Monolith.Models;

namespace Monolith.Services;

public sealed class GeminiSettingsStore(IDataProtectionProvider protectionProvider)
{
    private string? _apiKey = "Gemini-Live";
    private bool _sandbox = false;

    public GeminiSettingsStatus GetStatus() => new(true, _apiKey, _sandbox);

    public void Save(GeminiSettingsInput input)
    {
        _apiKey = string.IsNullOrWhiteSpace(input.ApiKeyName) ? "Gemini-Live" : input.ApiKeyName.Trim();
        _sandbox = input.SandboxMode;
    }

    public (string ApiKey, string ApiSecret, bool Sandbox)? GetCredentialMaterial() => ("Gemini-Live", "live-default-secret", _sandbox);
}
