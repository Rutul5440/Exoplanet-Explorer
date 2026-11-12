using System.Net;
using System.Net.Http.Json;
using ExoplanetExplorer.Api.Data;
using ExoplanetExplorer.Api.Models;
using ExoplanetExplorer.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExoplanetExplorer.Tests;

public sealed class ExoplanetApiIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly TestTapClient _tapClient = new();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        _tapClient.Rows =
        [
            new NasaTapRow("TRAPPIST-1 e", "TRAPPIST-1", "Transit", 2017, 0.92, 0.69, 6.10, 0.02925, 251, -3.28, 2566, 0.119, 0.089, 12.1),
            new NasaTapRow("Kepler-186 f", "Kepler-186", "Transit", 2014, 1.17, 1.71, 129.9, 0.432, 188, -1.26, 3755, 0.52, 0.54, 178),
            new NasaTapRow("51 Peg b", "51 Peg", "Radial Velocity", 1995, 14.0, 150.0, 4.23, 0.052, 1280, 0.12, 5793, 1.2, 1.11, 15.5)
        ];

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Sync:EnableStartupSeed"] = "false",
                    ["Sync:EnableBackgroundTimer"] = "false"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ExoplanetDbContext>>();
                services.AddDbContext<ExoplanetDbContext>(opts => opts.UseSqlite(_connection));

                services.RemoveAll<INasaTapClient>();
                services.AddSingleton<INasaTapClient>(_tapClient);
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExoplanetDbContext>();
        await db.Database.EnsureCreatedAsync();

        var sync = scope.ServiceProvider.GetRequiredService<ExoplanetSyncService>();
        await sync.SyncAsync("Startup");
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task GetExoplanets_WithRockyAndMethodFilters_ReturnsMatchingPlanets()
    {
        var response = await _client.GetFromJsonAsync<PagedResultDto<ExoplanetListItemDto>>(
            "/api/exoplanets?discoveryMethod=Transit&rockyOnly=true&habitabilityStatus=Conservative&sortBy=similarity&sortDir=desc");

        Assert.NotNull(response);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal("TRAPPIST-1 e", response.Items[0].PlanetName);
        Assert.Equal("Kepler-186 f", response.Items[1].PlanetName);
    }

    [Fact]
    public async Task GetExoplanets_WithInvertedRange_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/exoplanets?minMass=100&maxMass=10");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CompareExoplanets_ReturnsEarthBaselineAndRequestedPlanets()
    {
        var list = await _client.GetFromJsonAsync<PagedResultDto<ExoplanetListItemDto>>("/api/exoplanets?sortBy=name");
        Assert.NotNull(list);
        var ids = string.Join(',', list.Items.Take(2).Select(p => p.Id));

        var comparison = await _client.GetFromJsonAsync<ComparisonResponseDto>($"/api/exoplanets/compare?ids={ids}");

        Assert.NotNull(comparison);
        Assert.Equal("Earth (Reference)", comparison.EarthReference.PlanetName);
        Assert.Equal(2, comparison.Planets.Count);

        var tooMany = await _client.GetAsync("/api/exoplanets/compare?ids=1,2,3,4,5");
        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
    }

    [Fact]
    public async Task ChartsAndManualSync_InvalidateSummaryCacheImmediately()
    {
        var initialSummary = await _client.GetFromJsonAsync<CatalogSummaryDto>("/api/stats/summary");
        Assert.NotNull(initialSummary);
        Assert.Equal(3, initialSummary.TotalExoplanets);

        var scatter = await _client.GetFromJsonAsync<MassRadiusChartDto>("/api/charts/mass-radius");
        Assert.NotNull(scatter);
        Assert.Equal(3, scatter.TotalMatching);

        var trends = await _client.GetFromJsonAsync<DiscoveryTrendsDto>("/api/charts/discovery-trends");
        Assert.NotNull(trends);
        Assert.Contains("Transit", trends.Methods);
        Assert.Contains("Radial Velocity", trends.Methods);

        // Add a fourth planet and trigger manual sync; cached summary must reflect 4 immediately.
        _tapClient.Rows =
        [
            .. _tapClient.Rows,
            new NasaTapRow("Proxima Cen b", "Proxima Cen", "Radial Velocity", 2016, 1.07, 1.07, 11.186, 0.04856, 234, -2.81, 3042, 0.141, 0.122, 1.30)
        ];

        var triggerResp = await _client.PostAsync("/api/sync/trigger", content: null);
        Assert.Equal(HttpStatusCode.OK, triggerResp.StatusCode);

        var updatedSummary = await _client.GetFromJsonAsync<CatalogSummaryDto>("/api/stats/summary");
        Assert.NotNull(updatedSummary);
        Assert.Equal(4, updatedSummary.TotalExoplanets);
        Assert.Equal(3, updatedSummary.RockyHzCount);
    }

    private sealed class TestTapClient : INasaTapClient
    {
        public IReadOnlyList<NasaTapRow> Rows { get; set; } = [];

        public Task<IReadOnlyList<NasaTapRow>> FetchConfirmedPlanetsAsync(CancellationToken ct = default) =>
            Task.FromResult(Rows);
    }
}
