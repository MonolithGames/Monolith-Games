namespace Monolith.Models;

public sealed record LiveOrderRequest(
    string ProductId,
    string Side,
    string OrderType,
    decimal? BaseSize,
    decimal? QuoteSize,
    decimal? LimitPrice,
    bool ConfirmLiveOrder,
    string? ClientOrderId);

public sealed record LiveOrderSubmission(
    string ClientOrderId,
    string? CoinbaseOrderId,
    string Status,
    string ProductId,
    string Side,
    string OrderType,
    decimal Notional,
    DateTime CreatedAtUtc);
