using Monolith.Analytics;
using Monolith.Cache;
using Monolith.Data;
using Monolith.Events;
using Monolith.Integration;

ModuleTests.Run();

static class ModuleTests
{
    public static void Run()
    {
        DataModuleStoresRecords();
        CacheModuleStoresValues();
        EventsModulePublishesMessages();
        IntegrationModuleReportsStatus();
        AnalyticsModuleTracksCounters();
        Console.WriteLine("Monolith module tests passed.");
    }

    private static void DataModuleStoresRecords()
    {
        var service = new DataService();
        service.Save(new DataRecord("weather", "cached", DateTimeOffset.UtcNow));
        Assert(service.Get("weather")?.Value == "cached", "Data module did not return its record.");
        Assert(service.GetStatus().Name == "Data", "Data module status is incorrect.");
    }

    private static void CacheModuleStoresValues()
    {
        ICacheProvider service = new CacheService(new MemoryCache());
        service.Set("weather", "cached");
        Assert(service.Get<string>("weather") == "cached", "Cache module did not return its value.");
        Assert(service.GetStatus().Name == "Cache", "Cache module status is incorrect.");
    }

    private static void EventsModulePublishesMessages()
    {
        var bus = new EventBus();
        var handler = new RecordingHandler();
        bus.Subscribe("weather.updated", handler);
        bus.PublishAsync(new EventMessage("weather.updated", null, DateTimeOffset.UtcNow)).GetAwaiter().GetResult();
        Assert(handler.Received, "Events module did not publish to its handler.");
        Assert(bus.GetStatus().Name == "Events", "Events module status is incorrect.");
    }

    private static void IntegrationModuleReportsStatus()
    {
        var service = new IntegrationService(new HttpClient());
        Assert(service.GetStatus().Name == "Integration", "Integration module status is incorrect.");
    }

    private static void AnalyticsModuleTracksCounters()
    {
        IAnalyticsProvider service = new AnalyticsService();
        service.Increment("requests");
        Assert(service.GetMetrics().Metrics.Single().Value == 1, "Analytics module did not track its counter.");
        Assert(service.GetStatus().Name == "Analytics", "Analytics module status is incorrect.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class RecordingHandler : IEventHandler
    {
        public bool Received { get; private set; }

        public Task HandleAsync(EventMessage message, CancellationToken cancellationToken = default)
        {
            Received = true;
            return Task.CompletedTask;
        }
    }
}