namespace Monolith.Models;

public sealed record GeminiSettingsStatus(bool IsConfigured, string? ApiKeyName, bool SandboxMode);

public sealed class GeminiSettingsInput
{
    public string ApiKeyName { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool SandboxMode { get; set; } = true;
}
