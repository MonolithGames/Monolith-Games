using Microsoft.EntityFrameworkCore;
using Monolith.Web.Data;
using Monolith.Web.Models;

namespace Monolith.Web.Services;

public sealed class PaperTradingService(
    IDbContextFactory<MonolithDbContext> dbFactory,
    IConfiguration configuration,
    AuditService audit)
{
    public PaperTradingStatus GetStatus()
    {
        var enabled = configuration.GetValue("PAPER_TRADING_ENABLED", true);
        var killSwitch = configuration.GetValue("TRADING_KILL_SWITCH", true);
        var startingCash = configuration.GetValue("PAPER_STARTING_CASH", 100000m);
        var maxOrder = configuration.GetValue("COINBASE_MAX_ORDER_NOTIONAL", 1000m);
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
        if (request.Quantity * request.LimitPrice > status.MaxOrderNotional)
            throw new InvalidOperationException("Order exceeds the configured paper-trading risk limit.");

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