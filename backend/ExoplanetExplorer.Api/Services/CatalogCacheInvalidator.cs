namespace ExoplanetExplorer.Api.Services;

/// <summary>
/// Thread-safe epoch counter used to invalidate cached catalog, summary, and chart entries immediately after a sync commits changes.
/// </summary>
public sealed class CatalogCacheInvalidator
{
    private long _epoch = 1;

    public long CurrentEpoch => Interlocked.Read(ref _epoch);

    public long AdvanceEpoch() => Interlocked.Increment(ref _epoch);
}

