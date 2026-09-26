namespace Monolith.Services;

public sealed record AiTradeInsight(string Asset, string Action, string Confidence, string Reasoning, DateTimeOffset Timestamp);

public sealed class AiTradingService
{
    public Task<List<AiTradeInsight>> GetInsightsAsync(CancellationToken cancellationToken)
    {
        var insights = new List<AiTradeInsight>
        {
            new("BTC", "ACCUMULATE", "88%", "Strong institutional accumulation observed across Coinbase & Kraken order books in Michigan region.", DateTimeOffset.UtcNow),
            new("ETH", "HOLD / DCA", "82%", "Network fee stabilization indicates healthy consolidation above support levels.", DateTimeOffset.UtcNow),
            new("SOL", "WATCH", "74%", "High volume velocity detected on Gemini spot pairs; awaiting volatility breakout.", DateTimeOffset.UtcNow)
        };
        return Task.FromResult(insights);
    }
}
