namespace Monolith.Models;

public sealed record CryptoComSettingsStatus(bool IsConfigured, string? ApiKeyName);

public sealed class CryptoComSettingsInput
{
    public string ApiKeyName { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}
