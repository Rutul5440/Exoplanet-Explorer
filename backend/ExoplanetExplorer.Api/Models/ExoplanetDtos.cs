namespace ExoplanetExplorer.Api.Models;

/// <summary>
/// Filter, sort, and pagination parameters accepted by catalog endpoints.
/// </summary>
public sealed class ExoplanetQueryParameters
{
    public string? Search { get; set; }
    public double? MinMass { get; set; }
    public double? MaxMass { get; set; }
    public double? MinRadius { get; set; }
    public double? MaxRadius { get; set; }
    public double? MinPeriod { get; set; }
    public double? MaxPeriod { get; set; }
    public string? DiscoveryMethod { get; set; }
    public string? HabitabilityStatus { get; set; }
    public bool? RockyOnly { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

/// <summary>
/// Summary row returned for catalog browsing and filtering.
/// </summary>
public sealed record ExoplanetListItemDto(
    int Id,
    string PlanetName,
    string HostName,
    string DiscoveryMethod,
    int? DiscoveryYear,
    double? MassEarth,
    double? RadiusEarth,
    double? OrbitalPeriodDays,
    double? SemiMajorAxisAu,
    bool SemiMajorAxisDerived,
    double? EquilibriumTempK,
    double? InsolationEarth,
    string HabitabilityStatus,
    bool IsRockyCandidate,
    double? EarthSimilarityScore);

/// <summary>
/// Complete planet, host star, and habitable-zone metrics returned for inspection and comparison.
/// </summary>
public sealed record ExoplanetDetailDto(
    int Id,
    string PlanetName,
    string HostName,
    string DiscoveryMethod,
    int? DiscoveryYear,
    double? MassEarth,
    double? RadiusEarth,
    double? OrbitalPeriodDays,
    double? SemiMajorAxisAu,
    bool SemiMajorAxisDerived,
    double? EquilibriumTempK,
    double? StellarLuminosityLogSolar,
    double? StellarLuminositySolar,
    bool LuminosityDerived,
    double? StellarTeffK,
    double? StellarRadiusSolar,
    double? StellarMassSolar,
    double? DistanceParsecs,
    double? HzInnerConservativeAu,
    double? HzOuterConservativeAu,
    double? HzInnerOptimisticAu,
    double? HzOuterOptimisticAu,
    double? InsolationEarth,
    string HabitabilityStatus,
    bool IsRockyCandidate,
    double? EarthSimilarityScore,
    DateTime LastSyncedUtc);

/// <summary>
/// Paginated response envelope for catalog queries.
/// </summary>
public sealed record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Side-by-side comparison payload including Earth as the reference world.
/// </summary>
public sealed record ComparisonResponseDto(
    ExoplanetDetailDto EarthReference,
    IReadOnlyList<ExoplanetDetailDto> Planets);

/// <summary>
/// Single point for the Mass vs. Radius scatter chart.
/// </summary>
public sealed record MassRadiusPointDto(
    int Id,
    string PlanetName,
    string HostName,
    double MassEarth,
    double RadiusEarth,
    double? OrbitalPeriodDays,
    string DiscoveryMethod,
    string HabitabilityStatus,
    bool IsRockyCandidate,
    double? EarthSimilarityScore);

/// <summary>
/// Mass vs. Radius scatter response with total matched count disclosure when sampled.
/// </summary>
public sealed record MassRadiusChartDto(
    int TotalMatching,
    int ReturnedCount,
    IReadOnlyList<MassRadiusPointDto> Points);

/// <summary>
/// Yearly discovery counts grouped by discovery method.
/// </summary>
public sealed record DiscoveryTrendPointDto(
    int Year,
    string DiscoveryMethod,
    int Count);

/// <summary>
/// Discovery trends response including ordered method breakdown.
/// </summary>
public sealed record DiscoveryTrendsDto(
    IReadOnlyList<string> Methods,
    IReadOnlyList<DiscoveryTrendPointDto> Points);

/// <summary>
/// Catalog-wide KPI and filter metadata summary.
/// </summary>
public sealed record CatalogSummaryDto(
    int TotalExoplanets,
    int ConservativeHzCount,
    int OptimisticHzCount,
    int RockyHzCount,
    int DistinctHostStars,
    int LatestDiscoveryYear,
    IReadOnlyList<string> DiscoveryMethods,
    DateTime? LastSyncedUtc);

/// <summary>
/// Sync run record surfaced by sync telemetry endpoints.
/// </summary>
public sealed record SyncRunDto(
    int Id,
    DateTime StartedUtc,
    DateTime? CompletedUtc,
    string TriggerType,
    string Status,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int UnchangedCount,
    string? ErrorMessage);

/// <summary>
/// Current sync status, busy indicator, and recent sync history.
/// </summary>
public sealed record SyncStatusResponseDto(
    bool IsSyncRunning,
    SyncRunDto? LatestRun,
    IReadOnlyList<SyncRunDto> RecentRuns);
