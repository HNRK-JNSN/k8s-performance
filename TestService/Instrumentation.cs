
using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// It is recommended to use a custom type to hold references for
/// ActivitySource and Instruments. This avoids possible type collisions
/// with other components in the DI container.
/// </summary>
public class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "Weatherforecast.Service";
    internal const string MeterName = "Weatherforecast.Service.Metrics";
    private readonly Meter meter;

    public Instrumentation()
    {
        string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
        this.ActivitySource = new ActivitySource(ActivitySourceName, version);
        this.meter = new Meter(MeterName, version);
        this.CacheHitCounter = this.meter.CreateCounter<long>("weatherforecast.cache_hits", description: "hits on the cached data");
        this.CacheMissCounter = this.meter.CreateCounter<long>("weatherforecast.cache_misses", description: "misses on the cached data");
    }

    public ActivitySource ActivitySource { get; }

    public Counter<long> CacheHitCounter { get; }
    public Counter<long> CacheMissCounter { get; }

    public void Dispose()
    {
        this.ActivitySource.Dispose();
        this.meter.Dispose();
    }
}