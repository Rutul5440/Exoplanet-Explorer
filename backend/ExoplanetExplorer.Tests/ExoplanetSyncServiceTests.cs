using ExoplanetExplorer.Api.Data;
using ExoplanetExplorer.Api.Models;
using ExoplanetExplorer.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExoplanetExplorer.Tests;

public sealed class ExoplanetSyncServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private DbContextOptions<ExoplanetDbContext> _dbOptions = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _dbOptions = new DbContextOptionsBuilder<ExoplanetDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using var db = new ExoplanetDbContext(_dbOptions);
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task SyncAsync_InsertsDeduplicatesAndSkipsUnchangedOnRerun()
    {
        var feed = new StubTapClient(
        [
            new NasaTapRow("Kepler-442 b", "Kepler-442", "Transit", 2015, 1.34, 2.36, 112.3, 0.409, 233, -0.78, 4402, 0.60, 0.61, 370),
            new NasaTapRow("Kepler-442 b", "Kepler-442", "Transit", 2015, 1.35, 2.36, 112.3, 0.409, 233, -0.78, 4402, 0.60, 0.61, 370),
            new NasaTapRow("   ", "Bad Host", "Transit", 2020, 1.0, 1.0, 10, 0.1, 500, 0, 5000, 1, 1, 10),
            new NasaTapRow("51 Peg b", "51 Peg", "Radial Velocity", 1995, 14.0, 150.0, 4.23, 0.052, 1280, 0.12, 5793, 1.2, 1.11, 15.5)
        ]);

        var coordinator = new SyncCoordinator();
        var invalidator = new CatalogCacheInvalidator();

        await using (var db = new ExoplanetDbContext(_dbOptions))
        {
            var svc = new ExoplanetSyncService(db, feed, coordinator, invalidator, NullLogger<ExoplanetSyncService>.Instance);
            var first = await svc.SyncAsync("Manual");

            Assert.True(first.Accepted);
            Assert.NotNull(first.Run);
            Assert.Equal("Succeeded", first.Run.Status);
            Assert.Equal(2, first.Run.FetchedCount);
            Assert.Equal(2, first.Run.InsertedCount);
            Assert.Equal(0, first.Run.UpdatedCount);
            Assert.Equal(2, invalidator.CurrentEpoch);
        }

        // Second sync with identical rows should mark all unchanged and not rotate cache epoch.
        await using (var db = new ExoplanetDbContext(_dbOptions))
        {
            var svc = new ExoplanetSyncService(db, feed, coordinator, invalidator, NullLogger<ExoplanetSyncService>.Instance);
            var second = await svc.SyncAsync("Nightly");

            Assert.True(second.Accepted);
            Assert.NotNull(second.Run);
            Assert.Equal(0, second.Run.InsertedCount);
            Assert.Equal(0, second.Run.UpdatedCount);
            Assert.Equal(2, second.Run.UnchangedCount);
            Assert.Equal(2, invalidator.CurrentEpoch);
        }

        // Third sync mutating one planet updates only that planet and advances cache epoch.
        feed.Rows =
        [
            new NasaTapRow("Kepler-442 b", "Kepler-442", "Transit", 2015, 1.36, 2.36, 112.3, 0.409, 233, -0.78, 4402, 0.60, 0.61, 370),
            new NasaTapRow("51 Peg b", "51 Peg", "Radial Velocity", 1995, 14.0, 150.0, 4.23, 0.052, 1280, 0.12, 5793, 1.2, 1.11, 15.5)
        ];

        await using (var db = new ExoplanetDbContext(_dbOptions))
        {
            var svc = new ExoplanetSyncService(db, feed, coordinator, invalidator, NullLogger<ExoplanetSyncService>.Instance);
            var third = await svc.SyncAsync("Manual");

            Assert.True(third.Accepted);
            Assert.NotNull(third.Run);
            Assert.Equal(0, third.Run.InsertedCount);
            Assert.Equal(1, third.Run.UpdatedCount);
            Assert.Equal(1, third.Run.UnchangedCount);
            Assert.Equal(3, invalidator.CurrentEpoch);
        }
    }

    [Fact]
    public async Task SyncAsync_WhenAlreadyRunningAndWaitIfBusyFalse_ReturnsConflict()
    {
        var coordinator = new SyncCoordinator();
        Assert.True(await coordinator.TryAcquireAsync(waitIfBusy: false, CancellationToken.None));

        try
        {
            await using var db = new ExoplanetDbContext(_dbOptions);
            var svc = new ExoplanetSyncService(
                db,
                new StubTapClient([]),
                coordinator,
                new CatalogCacheInvalidator(),
                NullLogger<ExoplanetSyncService>.Instance);

            var result = await svc.SyncAsync("Manual", waitIfBusy: false);

            Assert.False(result.Accepted);
            Assert.Contains("already in progress", result.ConflictReason);
        }
        finally
        {
            coordinator.Release();
        }
    }

    [Fact]
    public async Task SyncAsync_WhenFeedReturnsEmpty_LogsFailureWithoutDeletingExistingRows()
    {
        var coordinator = new SyncCoordinator();
        var invalidator = new CatalogCacheInvalidator();

        await using (var db = new ExoplanetDbContext(_dbOptions))
        {
            var seedSvc = new ExoplanetSyncService(
                db,
                new StubTapClient([new NasaTapRow("Kepler-186 f", "Kepler-186", "Transit", 2014, 1.17, 1.71, 129.9, 0.432, 188, -1.26, 3755, 0.52, 0.54, 178)]),
                coordinator,
                invalidator,
                NullLogger<ExoplanetSyncService>.Instance);
            await seedSvc.SyncAsync("Startup");
        }

        await using (var db = new ExoplanetDbContext(_dbOptions))
        {
            var failSvc = new ExoplanetSyncService(
                db,
                new StubTapClient([]),
                coordinator,
                invalidator,
                NullLogger<ExoplanetSyncService>.Instance);
            var failed = await failSvc.SyncAsync("Manual");

            Assert.True(failed.Accepted);
            Assert.NotNull(failed.Run);
            Assert.Equal("Failed", failed.Run.Status);
            Assert.Equal(1, await db.Exoplanets.CountAsync());
        }
    }

    private sealed class StubTapClient(IReadOnlyList<NasaTapRow> initialRows) : INasaTapClient
    {
        public IReadOnlyList<NasaTapRow> Rows { get; set; } = initialRows;

        public Task<IReadOnlyList<NasaTapRow>> FetchConfirmedPlanetsAsync(CancellationToken ct = default) =>
            Task.FromResult(Rows);
    }
}

