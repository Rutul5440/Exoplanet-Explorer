using ExoplanetExplorer.Api.Services;

namespace ExoplanetExplorer.Tests;

public sealed class HabitableZoneCalculatorTests
{
    [Fact]
    public void Evaluate_EarthBaseline_ReturnsConservativeRockyWithPerfectSimilarity()
    {
        var result = HabitableZoneCalculator.Evaluate(new HabitableZoneInput(
            StellarLuminosityLogSolar: 0.0,
            StellarTeffK: 5772.0,
            StellarRadiusSolar: 1.0,
            StellarMassSolar: 1.0,
            SemiMajorAxisAu: 1.0,
            OrbitalPeriodDays: 365.25,
            PlanetRadiusEarth: 1.0,
            EquilibriumTempK: 255.0));

        Assert.Equal("Conservative", result.HabitabilityStatus);
        Assert.True(result.IsRockyCandidate);
        Assert.False(result.LuminosityDerived);
        Assert.False(result.SemiMajorAxisDerived);
        Assert.Equal(1.0, result.StellarLuminositySolar);
        Assert.Equal(1.0, result.InsolationEarth);
        Assert.Equal(100.0, result.EarthSimilarityScore);
    }

    [Fact]
    public void Evaluate_NegativeLogLuminosityRedDwarf_ComputesLinearSolarLuminosityAndHz()
    {
        // TRAPPIST-1 e representative values: log10(L) ~= -3.28, Teff = 2566 K, a = 0.02925 AU, R = 0.92 R_earth.
        var result = HabitableZoneCalculator.Evaluate(new HabitableZoneInput(
            StellarLuminosityLogSolar: -3.28,
            StellarTeffK: 2566.0,
            StellarRadiusSolar: 0.119,
            StellarMassSolar: 0.089,
            SemiMajorAxisAu: 0.02925,
            OrbitalPeriodDays: 6.1,
            PlanetRadiusEarth: 0.92,
            EquilibriumTempK: 251.0));

        Assert.NotNull(result.StellarLuminositySolar);
        Assert.InRange(result.StellarLuminositySolar.Value, 0.0005, 0.00056);
        Assert.Equal("Conservative", result.HabitabilityStatus);
        Assert.True(result.IsRockyCandidate);
        Assert.NotNull(result.InsolationEarth);
        Assert.InRange(result.InsolationEarth.Value, 0.55, 0.75);
    }

    [Fact]
    public void Evaluate_MissingLogLuminosityAndSemiMajorAxis_UsesStefanBoltzmannAndKeplerFallbacks()
    {
        var result = HabitableZoneCalculator.Evaluate(new HabitableZoneInput(
            StellarLuminosityLogSolar: null,
            StellarTeffK: 5772.0,
            StellarRadiusSolar: 1.0,
            StellarMassSolar: 1.0,
            SemiMajorAxisAu: null,
            OrbitalPeriodDays: 365.25,
            PlanetRadiusEarth: 1.1,
            EquilibriumTempK: null));

        Assert.True(result.LuminosityDerived);
        Assert.True(result.SemiMajorAxisDerived);
        Assert.Equal(1.0, result.StellarLuminositySolar);
        Assert.Equal(1.0, result.SemiMajorAxisAu);
        Assert.Equal("Conservative", result.HabitabilityStatus);
    }

    [Fact]
    public void Evaluate_GasGiantInHabitableZone_FlagsNotRocky()
    {
        var result = HabitableZoneCalculator.Evaluate(new HabitableZoneInput(
            StellarLuminosityLogSolar: 0.0,
            StellarTeffK: 5772.0,
            StellarRadiusSolar: 1.0,
            StellarMassSolar: 1.0,
            SemiMajorAxisAu: 1.05,
            OrbitalPeriodDays: 380.0,
            PlanetRadiusEarth: 11.2,
            EquilibriumTempK: 250.0));

        Assert.Equal("Conservative", result.HabitabilityStatus);
        Assert.False(result.IsRockyCandidate);
    }

    [Fact]
    public void Evaluate_NonFiniteAndNegativeInputs_DoesNotThrowAndReturnsUnknown()
    {
        var result = HabitableZoneCalculator.Evaluate(new HabitableZoneInput(
            StellarLuminosityLogSolar: double.NaN,
            StellarTeffK: -100.0,
            StellarRadiusSolar: double.PositiveInfinity,
            StellarMassSolar: 0.0,
            SemiMajorAxisAu: -1.0,
            OrbitalPeriodDays: double.NaN,
            PlanetRadiusEarth: -2.0,
            EquilibriumTempK: null));

        Assert.Equal("Unknown", result.HabitabilityStatus);
        Assert.False(result.IsRockyCandidate);
        Assert.Null(result.StellarLuminositySolar);
        Assert.Null(result.SemiMajorAxisAu);
        Assert.Null(result.EarthSimilarityScore);
    }
}

