using ExoplanetExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExoplanetExplorer.Api.Data.Configurations;

/// <summary>
/// EF Core schema and index configuration for <see cref="Exoplanet"/>.
/// </summary>
public sealed class ExoplanetConfiguration : IEntityTypeConfiguration<Exoplanet>
{
    public void Configure(EntityTypeBuilder<Exoplanet> builder)
    {
        builder.ToTable("Exoplanets");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PlanetName).HasMaxLength(120).IsRequired();
        builder.Property(e => e.HostName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DiscoveryMethod).HasMaxLength(64).IsRequired();
        builder.Property(e => e.HabitabilityStatus).HasMaxLength(32).IsRequired();
        builder.Property(e => e.ContentHash).HasMaxLength(64).IsUnicode(false).IsRequired();

        builder.HasIndex(e => e.PlanetName).IsUnique().HasDatabaseName("UX_Exoplanets_PlanetName");
        builder.HasIndex(e => e.HostName).HasDatabaseName("IX_Exoplanets_HostName");
        builder.HasIndex(e => e.DiscoveryMethod).HasDatabaseName("IX_Exoplanets_DiscoveryMethod");
        builder.HasIndex(e => e.DiscoveryYear).HasDatabaseName("IX_Exoplanets_DiscoveryYear");
        builder.HasIndex(e => new { e.HabitabilityStatus, e.IsRockyCandidate })
            .HasDatabaseName("IX_Exoplanets_Habitability_Rocky");
        builder.HasIndex(e => e.MassEarth).HasDatabaseName("IX_Exoplanets_MassEarth");
        builder.HasIndex(e => e.RadiusEarth).HasDatabaseName("IX_Exoplanets_RadiusEarth");
        builder.HasIndex(e => e.OrbitalPeriodDays).HasDatabaseName("IX_Exoplanets_OrbitalPeriodDays");
    }
}

