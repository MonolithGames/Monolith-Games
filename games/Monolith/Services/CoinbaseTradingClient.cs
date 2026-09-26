using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Monolith.Models;

namespace Monolith.Services;

public sealed class CoinbaseTradingClient(
    IHttpClientFactory httpClientFactory,
    CoinbaseSettingsStore settings,
    TradingPolicy policy,
    TradingLimits limits,
    IConfiguration configuration,
    AuditService audit,
    ILogger<CoinbaseTradingClient> logger)
{
    private const string CreateOrderPath = "/api/v3/brokerage/orders";
    private const string BaseUrl = "https://api.coinbase.com";

    public async Task<LiveOrderSubmission> PlaceOrderAsync(
        string actor,
        LiveOrderRequest request,
        CancellationToken cancellationToken)
    {
        var status = policy.GetStatus();
        if (status.BlockingReasons.Length > 0)
            throw new InvalidOperationException(string.Join("; ", status.BlockingReasons));
        if (configuration.GetValue("TRADING_KILL_SWITCH", true))
            throw new InvalidOperationException("Trading kill switch is active.");
        if (!request.ConfirmLiveOrder)
            throw new InvalidOperationException("Live orders require explicit confirmation.");

        var productId = request.ProductId.Trim().ToUpperInvariant();
        var side = request.Side.Trim().ToUpperInvariant();
        var orderType = request.OrderType.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(productId))
            throw new InvalidOperationException("Product is required.");
        if (side is not ("BUY" or "SELL"))
            throw new InvalidOperationException("Side must be BUY or SELL.");

        var clientOrderId = string.IsNullOrWhiteSpace(request.ClientOrderId)
            ? Guid.NewGuid().ToString("N")
            : request.ClientOrderId.Trim();
        var orderConfiguration = BuildConfiguration(orderType, request, out var notional);
        limits.ValidateNotional(notional, status.MaxOrderNotional);

        var material = settings.GetCredentialMaterial()
            ?? throw new InvalidOperationException("Coinbase credentials are not configured.");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BaseUrl + CreateOrderPath);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", CoinbaseJwt.Create(material.ApiKeyName, material.PrivateKey, "POST", CreateOrderPath));
        httpRequest.Content = JsonContent.Create(new
        {
            client_order_id = clientOrderId,
            product_id = productId,
            side,
            order_configuration = orderConfiguration
        });

        var client = httpClientFactory.CreateClient("coinbase");
        using var response = await client.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Coinbase live order failed with {Status}: {Body}", response.StatusCode, body);
            audit.Record(actor, "coinbase.order.create", productId, false);
            throw new HttpRequestException($"Coinbase returned {(int)response.StatusCode} for order {clientOrderId}.");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        string? orderId = null;
        var responseStatus = "SUBMITTED";
        if (root.TryGetProperty("success_response", out var success)
            && success.ValueKind is JsonValueKind.Object)
        {
            if (success.TryGetProperty("order_id", out var id))
                orderId = id.GetString();
            if (success.TryGetProperty("status", out var statusValue))
                responseStatus = statusValue.GetString() ?? responseStatus;
        }
        audit.Record(actor, "coinbase.order.create", productId, true);
        return new(clientOrderId, orderId, responseStatus, productId, side, orderType, notional, DateTime.UtcNow);
    }

    private object BuildConfiguration(string orderType, LiveOrderRequest request, out decimal notional)
    {
        switch (orderType)
        {
            case "market_market_ioc":
                if (request.QuoteSize is not > 0m || request.BaseSize is not null)
                    throw new InvalidOperationException("Market IOC orders require a positive quote size only.");
                notional = request.QuoteSize.Value;
                return new { market_market_ioc = new { quote_size = Format(request.QuoteSize.Value) } };
            case "limit_limit_gtc":
                if (request.BaseSize is not > 0m || request.LimitPrice is not > 0m || request.QuoteSize is not null)
                    throw new InvalidOperationException("Limit GTC orders require base size and limit price only.");
                notional = request.BaseSize.Value * request.LimitPrice.Value;
                return new { limit_limit_gtc = new { base_size = Format(request.BaseSize.Value), limit_price = Format(request.LimitPrice.Value), post_only = false } };
            default:
                throw new InvalidOperationException("Supported live order types are market_market_ioc and limit_limit_gtc.");
        }
    }

    private static string Format(decimal value) => value.ToString("0.################", CultureInfo.InvariantCulture);

}
