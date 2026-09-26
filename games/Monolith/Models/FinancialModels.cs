namespace Monolith.Models;

public sealed record AssetBalance(string Asset, decimal Available, decimal Held, string Currency);

public sealed record FinancialSnapshot(
    IReadOnlyList<AssetBalance> Balances,
    decimal? PortfolioTotal,
    DateTime RefreshedAtUtc,
    bool IsStale,
    string? Error);
