using System.Text.Json.Serialization;

namespace ExoplanetExplorer.Api.Models;

/// <summary>
/// Raw JSON row projected from the NASA Exoplanet Archive TAP <c>pscomppars</c> table.
/// </summary>
public sealed record NasaTapRow(
    [property: JsonPropertyName("pl_name")] string? PlanetName,
    [property: JsonPropertyName("hostname")] string? HostName,
    [property: JsonPropertyName("discoverymethod")] string? DiscoveryMethod,
    [property: JsonPropertyName("disc_year")] int? DiscoveryYear,
    [property: JsonPropertyName("pl_rade")] double? RadiusEarth,
    [property: JsonPropertyName("pl_bmasse")] double? MassEarth,
    [property: JsonPropertyName("pl_orbper")] double? OrbitalPeriodDays,
    [property: JsonPropertyName("pl_orbsmax")] double? SemiMajorAxisAu,
    [property: JsonPropertyName("pl_eqt")] double? EquilibriumTempK,
    [property: JsonPropertyName("st_lum")] double? StellarLuminosityLogSolar,
    [property: JsonPropertyName("st_teff")] double? StellarTeffK,
    [property: JsonPropertyName("st_rad")] double? StellarRadiusSolar,
    [property: JsonPropertyName("st_mass")] double? StellarMassSolar,
    [property: JsonPropertyName("sy_dist")] double? DistanceParsecs);

