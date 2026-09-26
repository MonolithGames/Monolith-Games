using System.Net;
using Monolith.Shared;

namespace Monolith.Integration;

public sealed record IntegrationRequest(HttpMethod Method, Uri Uri, HttpContent? Content = null);
public sealed record IntegrationResponse(HttpStatusCode StatusCode, string Content);

public sealed class IntegrationOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);
    public int RetryCount { get; set; }
    public bool Enabled { get; set; }
    public string ProviderName { get; set; } = "development-weather";
    public string? BaseUrl { get; set; }
    public string UserAgent { get; set; } = "Monolith/1.0";
}

public sealed record IntegrationProviderStatus(string Name, bool Enabled, string State, string? BaseUrl);
public sealed record IntegrationRefreshResult(string CorrelationId, string State, DateTimeOffset CompletedUtc, string? Error);

public interface IIntegrationClient
{
    Task<IntegrationResponse> SendAsync(IntegrationRequest request, CancellationToken cancellationToken = default);

    PlatformComponentStatus GetStatus();
    IntegrationProviderStatus GetProviderStatus();
    Task<IntegrationRefreshResult> RefreshWeatherAsync(
        Monolith.Cache.ICacheProvider cache,
        Monolith.Data.IDataProvider data,
        Monolith.Events.IEventBus events,
        Monolith.Analytics.IAnalyticsProvider analytics,
        CancellationToken cancellationToken = default);
}