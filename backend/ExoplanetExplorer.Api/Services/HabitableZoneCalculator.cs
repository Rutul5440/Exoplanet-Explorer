namespace ExoplanetExplorer.Api.Services;

/// <summary>
/// Input astrophysical parameters required to estimate planetary habitable-zone boundaries.
/// </summary>
public readonly record struct HabitableZoneInput(
    double? StellarLuminosityLogSolar,
    double? StellarTeffK,
    double? StellarRadiusSolar,
    double? StellarMassSolar,
    double? SemiMajorAxisAu,
    double? OrbitalPeriodDays,
    double? PlanetRadiusEarth,
    double? EquilibriumTempK);

/// <summary>
/// Computed habitable-zone boundaries, insolation flux, and habitability classification.
/// </summary>
public sealed record HabitableZoneEstimate(
    double? StellarLuminositySolar,
    bool LuminosityDerived,
    double? SemiMajorAxisAu,
    bool SemiMajorAxisDerived,
    double? HzInnerConservativeAu,
    double? HzOuterConservativeAu,
    double? HzInnerOptimisticAu,
    double? HzOuterOptimisticAu,
    double? InsolationEarth,
    string HabitabilityStatus,
    bool IsRockyCandidate,
    double? EarthSimilarityScore);

/// <summary>
/// Estimates conservative and optimistic habitable-zone boundaries using Kopparapu et al. (2013/2014)
/// stellar flux polynomials paired with NASA TAP stellar luminosity and orbital parameters.
/// </summary>
public static class HabitableZoneCalculator
{
    public const string FormulaVersion = "hz-v1";
    private const double SolarTeffK = 5772.0;
    private const double KopparapuTeffOriginK = 5780.0;
    private const double EarthEquilibriumTempK = 255.0;
    private const double RockyRadiusCutoffEarth = 1.8;

    public static HabitableZoneEstimate Evaluate(HabitableZoneInput input)
    {
        var (luminositySolar, luminosityDerived) = ResolveLuminositySolar(
            input.StellarLuminosityLogSolar,
            input.StellarTeffK,
            input.StellarRadiusSolar);

        var (semiMajorAxisAu, semiMajorAxisDerived) = ResolveSemiMajorAxisAu(
            input.SemiMajorAxisAu,
            input.OrbitalPeriodDays,
            input.StellarMassSolar);

        var planetRadius = FinitePositive(input.PlanetRadiusEarth);
        var isRocky = planetRadius is > 0 and <= RockyRadiusCutoffEarth;

        double? innerCons = null;
        double? outerCons = null;
        double? innerOpt = null;
        double? outerOpt = null;
        double? insolation = null;
        var status = "Unknown";

        if (luminositySolar is > 0)
        {
            var tStar = Math.Clamp(FinitePositive(input.StellarTeffK) ?? KopparapuTeffOriginK, 2600.0, 7200.0)
                        - KopparapuTeffOriginK;

            var sRecentVenus = EvaluateKopparapuFlux(1.776, 2.136e-4, 2.533e-8, -1.332e-11, -3.097e-15, tStar);
            var sRunaway = EvaluateKopparapuFlux(1.107, 1.332e-4, 1.580e-8, -8.308e-12, -1.931e-15, tStar);
            var sMaxGreenhouse = EvaluateKopparapuFlux(0.356, 6.171e-5, 1.698e-9, -3.198e-12, -5.575e-16, tStar);
            var sEarlyMars = EvaluateKopparapuFlux(0.320, 5.547e-5, 1.526e-9, -2.874e-12, -5.011e-16, tStar);

            var l = luminositySolar.Value;
            innerOpt = RoundOrNull(Math.Sqrt(l / sRecentVenus), 4);
            innerCons = RoundOrNull(Math.Sqrt(l / sRunaway), 4);
            outerCons = RoundOrNull(Math.Sqrt(l / sMaxGreenhouse), 4);
            outerOpt = RoundOrNull(Math.Sqrt(l / sEarlyMars), 4);

            if (semiMajorAxisAu is > 0)
            {
                var a = semiMajorAxisAu.Value;
                insolation = RoundOrNull(l / (a * a), 4);

                if (a >= innerCons && a <= outerCons)
                {
                    status = "Conservative";
                }
                else if (a >= innerOpt && a <= outerOpt)
                {
                    status = "Optimistic";
                }
                else
                {
                    status = "Outside";
                }
            }
        }

        var similarityScore = ComputeEarthSimilarityScore(planetRadius, insolation, FinitePositive(input.EquilibriumTempK));

        return new HabitableZoneEstimate(
            RoundOrNull(luminositySolar, 6),
            luminosityDerived,
            RoundOrNull(semiMajorAxisAu, 5),
            semiMajorAxisDerived,
            innerCons,
            outerCons,
            innerOpt,
            outerOpt,
            insolation,
            status,
            isRocky,
            similarityScore);
    }

    private static (double? Luminosity, bool Derived) ResolveLuminositySolar(
        double? logSolar,
        double? teffK,
        double? radiusSolar)
    {
        if (logSolar is double logVal && double.IsFinite(logVal) && logVal >= -8.0 && logVal <= 8.0)
        {
            return (Math.Pow(10.0, logVal), false);
        }

        var validRadius = FinitePositive(radiusSolar);
        var validTeff = FinitePositive(teffK);
        if (validRadius is > 0 && validTeff is > 0)
        {
            var ratio = validTeff.Value / SolarTeffK;
            var derived = validRadius.Value * validRadius.Value * ratio * ratio * ratio * ratio;
            if (double.IsFinite(derived) && derived > 0)
            {
                return (derived, true);
            }
        }

        return (null, false);
    }

    private static (double? SemiMajorAxisAu, bool Derived) ResolveSemiMajorAxisAu(
        double? semiMajorAxisAu,
        double? orbitalPeriodDays,
        double? stellarMassSolar)
    {
        var validA = FinitePositive(semiMajorAxisAu);
        if (validA is > 0)
        {
            return (validA, false);
        }

        var validPeriod = FinitePositive(orbitalPeriodDays);
        var validMass = FinitePositive(stellarMassSolar);
        if (validPeriod is > 0 && validMass is > 0)
        {
            var periodYears = validPeriod.Value / 365.25;
            var derivedA = Math.Cbrt(validMass.Value * periodYears * periodYears);
            if (double.IsFinite(derivedA) && derivedA > 0)
            {
                return (derivedA, true);
            }
        }

        return (null, false);
    }

    private static double EvaluateKopparapuFlux(double s0, double a, double b, double c, double d, double tStar)
    {
        var t2 = tStar * tStar;
        var t3 = t2 * tStar;
        var t4 = t2 * t2;
        return Math.Max(0.05, s0 + (a * tStar) + (b * t2) + (c * t3) + (d * t4));
    }

    private static double? ComputeEarthSimilarityScore(double? radiusEarth, double? insolationEarth, double? equilibriumTempK)
    {
        if (radiusEarth is not > 0)
        {
            return null;
        }

        var r = radiusEarth.Value;
        var radiusSimilarity = 1.0 - (Math.Abs(r - 1.0) / (r + 1.0));

        double? thermalSimilarity = null;
        if (insolationEarth is > 0)
        {
            var s = insolationEarth.Value;
            thermalSimilarity = 1.0 - (Math.Abs(s - 1.0) / (s + 1.0));
        }
        else if (equilibriumTempK is > 0)
        {
            var t = equilibriumTempK.Value;
            thermalSimilarity = 1.0 - (Math.Abs(t - EarthEquilibriumTempK) / (t + EarthEquilibriumTempK));
        }

        if (thermalSimilarity is null)
        {
            return null;
        }

        var score = Math.Sqrt(Math.Max(0.0, radiusSimilarity) * Math.Max(0.0, thermalSimilarity.Value)) * 100.0;
        return Math.Round(Math.Clamp(score, 0.0, 100.0), 1);
    }

    public static double? FinitePositive(double? value)
    {
        return value is double v && double.IsFinite(v) && v > 0 ? v : null;
    }

    private static double? RoundOrNull(double? value, int decimals)
    {
        return value is double v && double.IsFinite(v) ? Math.Round(v, decimals) : null;
    }
}

