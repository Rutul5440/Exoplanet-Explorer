namespace ExoplanetExplorer.Api.Data.Entities;

/// <summary>
/// Persisted confirmed exoplanet record enriched with computed habitable-zone estimates.
/// </summary>
public sealed class Exoplanet
{
    public int Id { get; set; }

    public string PlanetName { get; set; } = string.Empty;

    public string HostName { get; set; } = string.Empty;

    public string DiscoveryMethod { get; set; } = string.Empty;

    public int? DiscoveryYear { get; set; }

    public double? MassEarth { get; set; }

    public double? RadiusEarth { get; set; }

    public double? OrbitalPeriodDays { get; set; }

    public double? SemiMajorAxisAu { get; set; }

    public bool SemiMajorAxisDerived { get; set; }

    public double? EquilibriumTempK { get; set; }

    public double? StellarLuminosityLogSolar { get; set; }

    public double? StellarLuminositySolar { get; set; }

    public bool LuminosityDerived { get; set; }

    public double? StellarTeffK { get; set; }

    public double? StellarRadiusSolar { get; set; }

    public double? StellarMassSolar { get; set; }

    public double? DistanceParsecs { get; set; }

    public double? HzInnerConservativeAu { get; set; }

    public double? HzOuterConservativeAu { get; set; }

    public double? HzInnerOptimisticAu { get; set; }

    public double? HzOuterOptimisticAu { get; set; }

    public double? InsolationEarth { get; set; }

    public string HabitabilityStatus { get; set; } = "Unknown";

    public bool IsRockyCandidate { get; set; }

    public double? EarthSimilarityScore { get; set; }

    public string ContentHash { get; set; } = string.Empty;

    public DateTime LastSyncedUtc { get; set; }
}

