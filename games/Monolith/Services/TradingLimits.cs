namespace Monolith.Services;

public sealed class TradingLimits(IConfiguration configuration)
{
    public decimal MinimumOrderNotional => Math.Max(
        configuration.GetValue("COINBASE_MIN_ORDER_NOTIONAL", 2m), 2m);

    public decimal MaximumOrderNotional => configuration.GetValue("COINBASE_MAX_ORDER_NOTIONAL", 0m);

    public decimal PaperMaximumOrderNotional => MaximumOrderNotional > 0m
        ? MaximumOrderNotional
        : 1000m;

    public void ValidateNotional(decimal notional, decimal maximum)
    {
        if (notional < MinimumOrderNotional)
            throw new InvalidOperationException($"Order must be at least {MinimumOrderNotional:0.##} USD.");
        if (maximum <= 0m)
            throw new InvalidOperationException("Maximum order notional is not configured.");
        if (notional > maximum)
            throw new InvalidOperationException("Order exceeds the configured risk limit.");
    }
}
