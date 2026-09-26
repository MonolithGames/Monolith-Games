namespace Monolith.Models;

public sealed record KrakenSettingsStatus(bool IsConfigured, string? ApiKeyName, bool LiveTradingEnabled);

public sealed class KrakenSettingsInput
{
    public string ApiKeyName { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool LiveTradingEnabled { get; set; }
}
