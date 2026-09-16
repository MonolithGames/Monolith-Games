namespace Monolith.Web.Models;

public sealed record CoinbaseSettingsStatus(bool IsConfigured, string? ApiKeyName, bool LiveTradingEnabled, bool WithdrawalsEnabled);

public sealed class CoinbaseSettingsInput
{
	public string ApiKeyName { get; set; } = string.Empty;
	public string PrivateKey { get; set; } = string.Empty;
	public bool LiveTradingEnabled { get; set; }
	public bool WithdrawalsEnabled { get; set; }
}
