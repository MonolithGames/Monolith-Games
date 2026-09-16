namespace Monolith.Web.Models;

public sealed record TradingStatus(
    bool CredentialsConfigured,
    bool LiveTradingEnabled,
    bool WithdrawalsEnabled,
    bool RiskControlsReady,
    decimal MaxOrderNotional,
    string[] BlockingReasons);

public sealed record AuditEvent(Guid Id, string Actor, string Action, string Resource, bool Success, DateTime CreatedAtUtc);
