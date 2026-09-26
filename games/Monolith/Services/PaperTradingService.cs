using Microsoft.EntityFrameworkCore;
using Monolith.Data;
using Monolith.Models;

namespace Monolith.Services;

public sealed class PaperTradingService(
    IDbContextFactory<MonolithDbContext> dbFactory,
    IConfiguration configuration,
    AuditService audit,
    TradingLimits limits)
{
    public PaperTradingStatus GetStatus()
    {
        var enabled = configuration.GetValue("PAPER_TRADING_ENABLED", true);
        var killSwitch = configuration.GetValue("TRADING_KILL_SWITCH", true);
        var startingCash = configuration.GetValue("PAPER_STARTING_CASH", 100000m);
        var maxOrder = limits.PaperMaximumOrderNotional;
        var reasons = new List<string>();
        if (!enabled) reasons.Add("Paper trading is disabled");
        if (killSwitch) reasons.Add("Trading kill switch is active");
        if (maxOrder <= 0) reasons.Add("Maximum order notional is not positive");
        return new(enabled, killSwitch, startingCash, maxOrder, reasons.ToArray());
    }

    public IReadOnlyList<PaperOrder> GetOrders()
    {
        using var db = dbFactory.CreateDbContext();
        return db.PaperOrders.AsNoTracking().OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => new PaperOrder(order.Id, order.ProductId, order.Side, order.Quantity,
                order.LimitPrice, order.Status, order.CreatedAtUtc)).ToArray();
    }

    public PaperOrder PlaceOrder(string actor, PaperOrderRequest request)
    {
        var status = GetStatus();
        if (status.BlockingReasons.Length > 0)
            throw new InvalidOperationException(string.Join("; ", status.BlockingReasons));
        if (request.Side is not ("BUY" or "SELL"))
            throw new InvalidOperationException("Side must be BUY or SELL.");
        if (string.IsNullOrWhiteSpace(request.ProductId) || request.Quantity <= 0 || request.LimitPrice <= 0)
            throw new InvalidOperationException("Product, quantity, and limit price are required.");
        limits.ValidateNotional(request.Quantity * request.LimitPrice, status.MaxOrderNotional);

        using var db = dbFactory.CreateDbContext();
        var order = new PaperOrderEntity
        {
            Id = Guid.NewGuid(), ProductId = request.ProductId.Trim().ToUpperInvariant(), Side = request.Side,
            Quantity = request.Quantity, LimitPrice = request.LimitPrice, Status = "SIMULATED",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.PaperOrders.Add(order);
        db.SaveChanges();
        audit.Record(actor, "paper.order.created", order.ProductId, true);
        return new(order.Id, order.ProductId, order.Side, order.Quantity, order.LimitPrice, order.Status, order.CreatedAtUtc);
    }
}