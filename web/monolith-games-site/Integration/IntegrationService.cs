using System.Diagnostics;
using System.Text.Json;
using Monolith.Shared;

namespace Monolith.Integration;

public sealed class IntegrationService(HttpClient client, IntegrationOptions? options = null) : IIntegrationClient
{
    private readonly IntegrationOptions _options = options ?? new IntegrationOptions();

    public async Task<IntegrationResponse> SendAsync(IntegrationRequest request, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);
        using var message = new HttpRequestMessage(request.Method, request.Uri) { Content = request.Content };
        using var response = await client.SendAsync(message, timeout.Token);
        return new IntegrationResponse(response.StatusCode, await response.Content.ReadAsStringAsync(timeout.Token));
    }

    public PlatformComponentStatus GetStatus() =>
        new("Integration", "available", "/api/integration/status");

    public IntegrationProviderStatus GetProviderStatus() => new(
        _options.ProviderName,
        _options.Enabled,
        _options.Enabled && Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out _) ? "Configured" : "NotConfigured",
        _options.BaseUrl);

    public async Task<IntegrationRefreshResult> RefreshWeatherAsync(
        Monolith.Cache.ICacheProvider cache,
        Monolith.Data.IDataProvider data,
        Monolith.Events.IEventBus events,
        Monolith.Analytics.IAnalyticsProvider analytics,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var started = Stopwatch.GetTimestamp();
        if (!GetProviderStatus().State.Equals("Configured", StringComparison.Ordinal))
        {
            analytics.IncrementBounded("integration.failures");
            return new(correlationId, "NotConfigured", DateTimeOffset.UtcNow, "The weather provider is not configured.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_options.BaseUrl!, UriKind.Absolute));
            request.Headers.UserAgent.ParseAdd(_options.UserAgent);
            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = System.Text.Json.JsonDocument.Parse(content);
            var payload = document.RootElement.Clone();
            await cache.SetJsonAsync("integration.weather.latest", payload, TimeSpan.FromMinutes(15), cancellationToken);
            await data.UpsertAsync("integration.weather.latest", payload, cancellationToken);
            await events.PublishAsync(new Monolith.Events.EventEnvelope(Guid.NewGuid(), "IntegrationRefreshCompleted", correlationId, "Monolith.Integration", DateTimeOffset.UtcNow, payload), cancellationToken);
            analytics.IncrementBounded("integration.successes");
            analytics.Record("integration.last_duration_ms", Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            return new(correlationId, "Completed", DateTimeOffset.UtcNow, null);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException)
        {
            analytics.IncrementBounded("integration.failures");
            await events.PublishAsync(new Monolith.Events.EventEnvelope(Guid.NewGuid(), "IntegrationRefreshFailed", correlationId, "Monolith.Integration", DateTimeOffset.UtcNow, System.Text.Json.JsonDocument.Parse("{}").RootElement.Clone()), cancellationToken);
            return new(correlationId, "Failed", DateTimeOffset.UtcNow, "The weather provider request failed.");
        }
    }
}