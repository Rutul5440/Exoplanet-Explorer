using ExoplanetExplorer.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExoplanetExplorer.Api.Data;

/// <summary>
/// Entity Framework Core database context for confirmed exoplanet catalog and sync logs.
/// </summary>
public sealed class ExoplanetDbContext(DbContextOptions<ExoplanetDbContext> options) : DbContext(options)
{
    public DbSet<Exoplanet> Exoplanets => Set<Exoplanet>();

    public DbSet<SyncRun> SyncRuns => Set<SyncRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExoplanetDbContext).Assembly);
    }
}

