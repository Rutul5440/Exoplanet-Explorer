using ExoplanetExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExoplanetExplorer.Api.Data.Configurations;

/// <summary>
/// EF Core schema and index configuration for <see cref="SyncRun"/>.
/// </summary>
public sealed class SyncRunConfiguration : IEntityTypeConfiguration<SyncRun>
{
    public void Configure(EntityTypeBuilder<SyncRun> builder)
    {
        builder.ToTable("SyncRuns");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TriggerType).HasMaxLength(16).IsRequired();
        builder.Property(s => s.Status).HasMaxLength(16).IsRequired();
        builder.Property(s => s.ErrorMessage).HasMaxLength(1000);

        builder.HasIndex(s => s.StartedUtc).HasDatabaseName("IX_SyncRuns_StartedUtc");
    }
}

