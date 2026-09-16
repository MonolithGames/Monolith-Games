using Monolith.Web.Models;

namespace Monolith.Web.Services;

public sealed class TradingPolicy(IConfiguration configuration, CoinbaseSettingsStore coinbase)
{
    public TradingStatus GetStatus()
    {
        var settings = coinbase.GetStatus();
        var liveTrading = configuration.GetValue("COINBASE_LIVE_TRADING_ENABLED", false) && settings.LiveTradingEnabled;
        var maxOrder = configuration.GetValue("COINBASE_MAX_ORDER_NOTIONAL", 0m);
        var reasons = new List<string>();
        if (!settings.IsConfigured) reasons.Add("Coinbase credentials are not configured");
        if (!liveTrading) reasons.Add("Live trading is disabled");
        if (maxOrder <= 0) reasons.Add("Maximum order notional is not configured");
        if (settings.WithdrawalsEnabled) reasons.Add("Withdrawals must remain disabled");
        return new(settings.IsConfigured, liveTrading, settings.WithdrawalsEnabled, reasons.Count == 0, maxOrder, reasons.ToArray());
    }
}