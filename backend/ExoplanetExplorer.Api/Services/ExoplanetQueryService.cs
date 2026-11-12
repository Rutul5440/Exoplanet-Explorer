using System.Globalization;
using ExoplanetExplorer.Api.Data;
using ExoplanetExplorer.Api.Data.Entities;
using ExoplanetExplorer.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ExoplanetExplorer.Api.Services;

/// <summary>
/// Read-only query, aggregation, comparison, and in-memory caching service for the exoplanet catalog.
/// </summary>
public sealed class ExoplanetQueryService(
    ExoplanetDbContext db,
    IMemoryCache cache,
    CatalogCacheInvalidator cacheInvalidator)
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;
    private const int MaxScatterPoints = 2500;
    private const int MaxComparePlanets = 4;

    private static readonly HashSet<string> ValidHabitabilityStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Conservative",
        "Optimistic",
        "Outside",
        "Unknown"
    };

    private static readonly MemoryCacheEntryOptions DefaultCacheOptions = new MemoryCacheEntryOptions()
        .SetSize(1)
        .SetSlidingExpiration(TimeSpan.FromMinutes(15))
        .SetAbsoluteExpiration(TimeSpan.FromHours(6));

    public static string? ValidateFilters(ExoplanetQueryParameters query)
    {
        if (query.MinMass < 0 || query.MaxMass < 0)
            return "Mass bounds must be non-negative.";
        if (query.MinMass > query.MaxMass)
            return "minMass cannot exceed maxMass.";

        if (query.MinRadius < 0 || query.MaxRadius < 0)
            return "Radius bounds must be non-negative.";
        if (query.MinRadius > query.MaxRadius)
            return "minRadius cannot exceed maxRadius.";

        if (query.MinPeriod < 0 || query.MaxPeriod < 0)
            return "Orbital period bounds must be non-negative.";
        if (query.MinPeriod > query.MaxPeriod)
            return "minPeriod cannot exceed maxPeriod.";

        if (!string.IsNullOrWhiteSpace(query.HabitabilityStatus) &&
            !ValidHabitabilityStatuses.Contains(query.HabitabilityStatus.Trim()))
        {
            return "habitabilityStatus must be Conservative, Optimistic, Outside, or Unknown.";
        }

        return null;
    }

    public async Task<PagedResultDto<ExoplanetListItemDto>> GetPagedAsync(
        ExoplanetQueryParameters rawQuery,
        CancellationToken ct = default)
    {
        var page = Math.Max(1, rawQuery.Page ?? 1);
        var rawSize = rawQuery.PageSize ?? DefaultPageSize;
        var pageSize = Math.Clamp(rawSize <= 0 ? DefaultPageSize : rawSize, 1, MaxPageSize);
        var sortBy = (rawQuery.SortBy ?? "name").Trim().ToLowerInvariant();
        var desc = string.Equals(rawQuery.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        var cacheKey = $"v{cacheInvalidator.CurrentEpoch}:list:{BuildFilterCacheKey(rawQuery)}:p={page}:ps={pageSize}:sb={sortBy}:d={desc}";
        if (cache.TryGetValue(cacheKey, out PagedResultDto<ExoplanetListItemDto>? cached) && cached is not null)
        {
            return cached;
        }

        var filtered = ApplyFilters(db.Exoplanets.AsNoTracking(), rawQuery);
        var totalCount = await filtered.CountAsync(ct).ConfigureAwait(false);
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var safePage = Math.Min(page, totalPages);

        var ordered = ApplySorting(filtered, sortBy, desc);

        var items = await ordered
            .Skip((safePage - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExoplanetListItemDto(
                e.Id,
                e.PlanetName,
                e.HostName,
                e.DiscoveryMethod,
                e.DiscoveryYear,
                e.MassEarth,
                e.RadiusEarth,
                e.OrbitalPeriodDays,
                e.SemiMajorAxisAu,
                e.SemiMajorAxisDerived,
                e.EquilibriumTempK,
                e.InsolationEarth,
                e.HabitabilityStatus,
                e.IsRockyCandidate,
                e.EarthSimilarityScore))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var result = new PagedResultDto<ExoplanetListItemDto>(items, totalCount, safePage, pageSize, totalPages);
        cache.Set(cacheKey, result, DefaultCacheOptions);
        return result;
    }

    public async Task<ExoplanetDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
        {
            return null;
        }

        var cacheKey = $"v{cacheInvalidator.CurrentEpoch}:detail:{id}";
        if (cache.TryGetValue(cacheKey, out ExoplanetDetailDto? cached))
        {
            return cached;
        }

        var item = await db.Exoplanets
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => ToDetailDto(e))
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (item is not null)
        {
            cache.Set(cacheKey, item, DefaultCacheOptions);
        }

        return item;
    }

    public async Task<(ComparisonResponseDto? Response, string? Error)> CompareAsync(
        IReadOnlyList<int> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
        {
            return (null, "Provide at least 1 planet id to compare.");
        }

        // shortcut: max 4 planets compared side-by-side, upgrade if comparison matrix adds horizontal virtualization
        if (ids.Count > MaxComparePlanets)
        {
            return (null, $"You can compare up to {MaxComparePlanets} planets at a time.");
        }

        if (ids.Any(id => id <= 0))
        {
            return (null, "All planet ids must be positive integers.");
        }

        var distinctIds = ids.Distinct().ToList();
        var records = await db.Exoplanets
            .AsNoTracking()
            .Where(e => distinctIds.Contains(e.Id))
            .Select(e => ToDetailDto(e))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var byId = records.ToDictionary(p => p.Id);
        var ordered = new List<ExoplanetDetailDto>(distinctIds.Count);
        foreach (var id in distinctIds)
        {
            if (byId.TryGetValue(id, out var dto))
            {
                ordered.Add(dto);
            }
        }

        if (ordered.Count == 0)
        {
            return (null, "None of the requested planet ids were found.");
        }

        return (new ComparisonResponseDto(BuildEarthReference(), ordered), null);
    }

    public async Task<MassRadiusChartDto> GetMassRadiusChartAsync(
        ExoplanetQueryParameters rawQuery,
        CancellationToken ct = default)
    {
        var cacheKey = $"v{cacheInvalidator.CurrentEpoch}:chart:mr:{BuildFilterCacheKey(rawQuery)}";
        if (cache.TryGetValue(cacheKey, out MassRadiusChartDto? cached) && cached is not null)
        {
            return cached;
        }

        var query = ApplyFilters(db.Exoplanets.AsNoTracking(), rawQuery)
            .Where(e => e.MassEarth != null && e.MassEarth > 0 && e.RadiusEarth != null && e.RadiusEarth > 0);

        var totalMatching = await query.CountAsync(ct).ConfigureAwait(false);

        // shortcut: caps scatter points at 2500 keeping HZ candidates prioritized and spreading the remaining sample evenly across masses/radii; upgrade to server-side density binning if catalog exceeds 20k planets
        var points = await query
            .OrderByDescending(e => e.HabitabilityStatus == "Conservative" ? 2 : e.HabitabilityStatus == "Optimistic" ? 1 : 0)
            .ThenBy(e => (e.Id * 7919) % 10007)
            .ThenBy(e => e.Id)
            .Take(MaxScatterPoints)
            .Select(e => new MassRadiusPointDto(
                e.Id,
                e.PlanetName,
                e.HostName,
                e.MassEarth!.Value,
                e.RadiusEarth!.Value,
                e.OrbitalPeriodDays,
                e.DiscoveryMethod,
                e.HabitabilityStatus,
                e.IsRockyCandidate,
                e.EarthSimilarityScore))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var result = new MassRadiusChartDto(totalMatching, points.Count, points);
        cache.Set(cacheKey, result, DefaultCacheOptions);
        return result;
    }

    public async Task<DiscoveryTrendsDto> GetDiscoveryTrendsAsync(
        ExoplanetQueryParameters rawQuery,
        CancellationToken ct = default)
    {
        var cacheKey = $"v{cacheInvalidator.CurrentEpoch}:chart:trends:{BuildFilterCacheKey(rawQuery)}";
        if (cache.TryGetValue(cacheKey, out DiscoveryTrendsDto? cached) && cached is not null)
        {
            return cached;
        }

        var grouped = (await ApplyFilters(db.Exoplanets.AsNoTracking(), rawQuery)
            .Where(e => e.DiscoveryYear != null)
            .GroupBy(e => new { Year = e.DiscoveryYear!.Value, e.DiscoveryMethod })
            .Select(g => new { g.Key.Year, g.Key.DiscoveryMethod, Count = g.Count() })
            .OrderBy(p => p.Year)
            .ThenBy(p => p.DiscoveryMethod)
            .ToListAsync(ct)
            .ConfigureAwait(false))
            .Select(p => new DiscoveryTrendPointDto(p.Year, p.DiscoveryMethod, p.Count))
            .ToList();

        var methods = grouped
            .GroupBy(p => p.DiscoveryMethod)
            .OrderByDescending(g => g.Sum(x => x.Count))
            .Select(g => g.Key)
            .ToList();

        var result = new DiscoveryTrendsDto(methods, grouped);
        cache.Set(cacheKey, result, DefaultCacheOptions);
        return result;
    }

    public async Task<CatalogSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var cacheKey = $"v{cacheInvalidator.CurrentEpoch}:summary";
        if (cache.TryGetValue(cacheKey, out CatalogSummaryDto? cached) && cached is not null)
        {
            return cached;
        }

        var baseQuery = db.Exoplanets.AsNoTracking();

        // Aggregate status/rocky counts and max metadata in one grouped scan instead of separate queries.
        var statusBuckets = await baseQuery
            .GroupBy(e => new { e.HabitabilityStatus, e.IsRockyCandidate })
            .Select(g => new
            {
                g.Key.HabitabilityStatus,
                g.Key.IsRockyCandidate,
                Count = g.Count(),
                MaxYear = g.Max(x => x.DiscoveryYear),
                MaxSync = g.Max(x => x.LastSyncedUtc)
            })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var total = statusBuckets.Sum(b => b.Count);
        var conservativeHz = statusBuckets.Where(b => b.HabitabilityStatus == "Conservative").Sum(b => b.Count);
        var optimisticHz = statusBuckets.Where(b => b.HabitabilityStatus == "Optimistic").Sum(b => b.Count);
        var rockyHz = statusBuckets
            .Where(b => b.IsRockyCandidate && (b.HabitabilityStatus == "Conservative" || b.HabitabilityStatus == "Optimistic"))
            .Sum(b => b.Count);
        var latestYear = statusBuckets.Select(b => b.MaxYear).Max() ?? 0;
        DateTime? lastSyncedUtc = statusBuckets.Count == 0 ? null : statusBuckets.Max(b => b.MaxSync);

        var distinctHosts = await baseQuery.Select(e => e.HostName).Distinct().CountAsync(ct).ConfigureAwait(false);
        var methods = await baseQuery
            .GroupBy(e => e.DiscoveryMethod)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var summary = new CatalogSummaryDto(
            total,
            conservativeHz,
            optimisticHz,
            rockyHz,
            distinctHosts,
            latestYear,
            methods,
            lastSyncedUtc);

        cache.Set(cacheKey, summary, DefaultCacheOptions);
        return summary;
    }

    public async Task<SyncStatusResponseDto> GetSyncStatusAsync(bool isRunning, CancellationToken ct = default)
    {
        var recent = await db.SyncRuns
            .AsNoTracking()
            .OrderByDescending(s => s.StartedUtc)
            .ThenByDescending(s => s.Id)
            .Take(10)
            .Select(s => new SyncRunDto(
                s.Id,
                s.StartedUtc,
                s.CompletedUtc,
                s.TriggerType,
                s.Status,
                s.FetchedCount,
                s.InsertedCount,
                s.UpdatedCount,
                s.UnchangedCount,
                s.ErrorMessage))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return new SyncStatusResponseDto(isRunning, recent.FirstOrDefault(), recent);
    }

    private static IQueryable<Exoplanet> ApplyFilters(IQueryable<Exoplanet> query, ExoplanetQueryParameters p)
    {
        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var sanitized = p.Search.Trim().Replace("%", string.Empty).Replace("_", string.Empty).Replace("[", string.Empty);
            if (!string.IsNullOrEmpty(sanitized))
            {
                var pattern = $"%{sanitized}%";
                query = query.Where(e => EF.Functions.Like(e.PlanetName, pattern) || EF.Functions.Like(e.HostName, pattern));
            }
        }

        if (p.MinMass.HasValue)
            query = query.Where(e => e.MassEarth != null && e.MassEarth >= p.MinMass.Value);
        if (p.MaxMass.HasValue)
            query = query.Where(e => e.MassEarth != null && e.MassEarth <= p.MaxMass.Value);

        if (p.MinRadius.HasValue)
            query = query.Where(e => e.RadiusEarth != null && e.RadiusEarth >= p.MinRadius.Value);
        if (p.MaxRadius.HasValue)
            query = query.Where(e => e.RadiusEarth != null && e.RadiusEarth <= p.MaxRadius.Value);

        if (p.MinPeriod.HasValue)
            query = query.Where(e => e.OrbitalPeriodDays != null && e.OrbitalPeriodDays >= p.MinPeriod.Value);
        if (p.MaxPeriod.HasValue)
            query = query.Where(e => e.OrbitalPeriodDays != null && e.OrbitalPeriodDays <= p.MaxPeriod.Value);

        if (!string.IsNullOrWhiteSpace(p.DiscoveryMethod))
        {
            var method = p.DiscoveryMethod.Trim();
            query = query.Where(e => e.DiscoveryMethod == method);
        }

        if (!string.IsNullOrWhiteSpace(p.HabitabilityStatus))
        {
            var canonicalStatus = ValidHabitabilityStatuses.First(s =>
                string.Equals(s, p.HabitabilityStatus.Trim(), StringComparison.OrdinalIgnoreCase));
            query = query.Where(e => e.HabitabilityStatus == canonicalStatus);
        }

        if (p.RockyOnly == true)
        {
            query = query.Where(e => e.IsRockyCandidate);
        }

        return query;
    }

    private static IOrderedQueryable<Exoplanet> ApplySorting(IQueryable<Exoplanet> query, string sortBy, bool desc)
    {
        return (sortBy, desc) switch
        {
            ("host", true) => query.OrderByDescending(e => e.HostName).ThenBy(e => e.Id),
            ("host", false) => query.OrderBy(e => e.HostName).ThenBy(e => e.Id),
            ("method", true) => query.OrderByDescending(e => e.DiscoveryMethod).ThenBy(e => e.Id),
            ("method", false) => query.OrderBy(e => e.DiscoveryMethod).ThenBy(e => e.Id),
            ("year", true) => query.OrderByDescending(e => e.DiscoveryYear ?? int.MinValue).ThenBy(e => e.Id),
            ("year", false) => query.OrderBy(e => e.DiscoveryYear ?? int.MaxValue).ThenBy(e => e.Id),
            ("mass", true) => query.OrderByDescending(e => e.MassEarth ?? double.MinValue).ThenBy(e => e.Id),
            ("mass", false) => query.OrderBy(e => e.MassEarth ?? double.MaxValue).ThenBy(e => e.Id),
            ("radius", true) => query.OrderByDescending(e => e.RadiusEarth ?? double.MinValue).ThenBy(e => e.Id),
            ("radius", false) => query.OrderBy(e => e.RadiusEarth ?? double.MaxValue).ThenBy(e => e.Id),
            ("period", true) => query.OrderByDescending(e => e.OrbitalPeriodDays ?? double.MinValue).ThenBy(e => e.Id),
            ("period", false) => query.OrderBy(e => e.OrbitalPeriodDays ?? double.MaxValue).ThenBy(e => e.Id),
            ("semimajoraxis", true) => query.OrderByDescending(e => e.SemiMajorAxisAu ?? double.MinValue).ThenBy(e => e.Id),
            ("semimajoraxis", false) => query.OrderBy(e => e.SemiMajorAxisAu ?? double.MaxValue).ThenBy(e => e.Id),
            ("temp", true) => query.OrderByDescending(e => e.EquilibriumTempK ?? double.MinValue).ThenBy(e => e.Id),
            ("temp", false) => query.OrderBy(e => e.EquilibriumTempK ?? double.MaxValue).ThenBy(e => e.Id),
            ("insolation", true) => query.OrderByDescending(e => e.InsolationEarth ?? double.MinValue).ThenBy(e => e.Id),
            ("insolation", false) => query.OrderBy(e => e.InsolationEarth ?? double.MaxValue).ThenBy(e => e.Id),
            ("similarity", true) => query.OrderByDescending(e => e.EarthSimilarityScore ?? double.MinValue).ThenBy(e => e.Id),
            ("similarity", false) => query.OrderBy(e => e.EarthSimilarityScore ?? double.MaxValue).ThenBy(e => e.Id),
            ("name", true) => query.OrderByDescending(e => e.PlanetName).ThenBy(e => e.Id),
            _ => query.OrderBy(e => e.PlanetName).ThenBy(e => e.Id)
        };
    }

    private static string BuildFilterCacheKey(ExoplanetQueryParameters p)
    {
        return string.Join('|',
            p.Search?.Trim().ToUpperInvariant() ?? string.Empty,
            p.MinMass?.ToString("G", CultureInfo.InvariantCulture) ?? string.Empty,
            p.MaxMass?.ToString("G", CultureInfo.InvariantCulture) ?? string.Empty,
            p.MinRadius?.ToString("G", CultureInfo.InvariantCulture) ?? string.Empty,
            p.MaxRadius?.ToString("G", CultureInfo.InvariantCulture) ?? string.Empty,
            p.MinPeriod?.ToString("G", CultureInfo.InvariantCulture) ?? string.Empty,
            p.MaxPeriod?.ToString("G", CultureInfo.InvariantCulture) ?? string.Empty,
            p.DiscoveryMethod?.Trim() ?? string.Empty,
            p.HabitabilityStatus?.Trim().ToUpperInvariant() ?? string.Empty,
            p.RockyOnly == true ? "1" : "0");
    }

    private static ExoplanetDetailDto ToDetailDto(Exoplanet e) =>
        new(
            e.Id,
            e.PlanetName,
            e.HostName,
            e.DiscoveryMethod,
            e.DiscoveryYear,
            e.MassEarth,
            e.RadiusEarth,
            e.OrbitalPeriodDays,
            e.SemiMajorAxisAu,
            e.SemiMajorAxisDerived,
            e.EquilibriumTempK,
            e.StellarLuminosityLogSolar,
            e.StellarLuminositySolar,
            e.LuminosityDerived,
            e.StellarTeffK,
            e.StellarRadiusSolar,
            e.StellarMassSolar,
            e.DistanceParsecs,
            e.HzInnerConservativeAu,
            e.HzOuterConservativeAu,
            e.HzInnerOptimisticAu,
            e.HzOuterOptimisticAu,
            e.InsolationEarth,
            e.HabitabilityStatus,
            e.IsRockyCandidate,
            e.EarthSimilarityScore,
            e.LastSyncedUtc);

    public static ExoplanetDetailDto BuildEarthReference()
    {
        var hz = HabitableZoneCalculator.Evaluate(new HabitableZoneInput(
            StellarLuminosityLogSolar: 0.0,
            StellarTeffK: 5772.0,
            StellarRadiusSolar: 1.0,
            StellarMassSolar: 1.0,
            SemiMajorAxisAu: 1.0,
            OrbitalPeriodDays: 365.25,
            PlanetRadiusEarth: 1.0,
            EquilibriumTempK: 255.0));

        return new ExoplanetDetailDto(
            0,
            "Earth (Reference)",
            "Sun",
            "Solar System Baseline",
            null,
            1.0,
            1.0,
            365.25,
            hz.SemiMajorAxisAu,
            false,
            255.0,
            0.0,
            hz.StellarLuminositySolar,
            false,
            5772.0,
            1.0,
            1.0,
            0.0,
            hz.HzInnerConservativeAu,
            hz.HzOuterConservativeAu,
            hz.HzInnerOptimisticAu,
            hz.HzOuterOptimisticAu,
            hz.InsolationEarth,
            hz.HabitabilityStatus,
            hz.IsRockyCandidate,
            hz.EarthSimilarityScore,
            DateTime.UtcNow);
    }
}
