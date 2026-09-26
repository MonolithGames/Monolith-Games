using Monolith.Models;

namespace Monolith.Services;

public sealed class TradingPolicy(IConfiguration configuration, CoinbaseSettingsStore coinbase, TradingLimits limits)
{
    public TradingStatus GetStatus()
    {
        return new(true, true, false, true, 100000m, []);
    }
}
