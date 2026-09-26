namespace Monolith.Models;

public sealed record PaperOrder(
    Guid Id,
    string ProductId,
    string Side,
    decimal Quantity,
    decimal LimitPrice,
    string Status,
    DateTime CreatedAtUtc);

public sealed record PaperOrderRequest(string ProductId, string Side, decimal Quantity, decimal LimitPrice);

public sealed record PaperTradingStatus(bool Enabled, bool KillSwitchActive, decimal StartingCash, decimal MaxOrderNotional, string[] BlockingReasons);
