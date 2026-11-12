using ExoplanetExplorer.Api.Configuration;
using ExoplanetExplorer.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExoplanetExplorer.Api.Services;

/// <summary>
/// Background hosted service that seeds an empty catalog on startup and runs nightly NASA TAP synchronizations.
/// </summary>
public sealed class NightlySyncWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<SyncOptions> options,
    ILogger<NightlySyncWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.Value.EnableStartupSeed)
        {
            await SeedIfEmptyOrStaleAsync(stoppingToken).ConfigureAwait(false);
        }

        if (!options.Value.EnableBackgroundTimer)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = ComputeDelayUntilNextRunUtc(DateTime.UtcNow, options.Value.DailyRunHourUtc);
            logger.LogInformation("Next nightly NASA exoplanet sync scheduled in {Delay}.", delay);

            try
            {
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await RunScopedSyncAsync("Nightly", waitIfBusy: true, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task SeedIfEmptyOrStaleAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ExoplanetDbContext>();

            var hasCatalogData = await db.Exoplanets.AsNoTracking().AnyAsync(ct).ConfigureAwait(false);
            if (!hasCatalogData)
            {
                logger.LogInformation("Exoplanets table is empty; running initial NASA TAP seed synchronization.");
                var syncService = scope.ServiceProvider.GetRequiredService<ExoplanetSyncService>();
                await syncService.SyncAsync("Startup", waitIfBusy: false, ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Normal shutdown during initial check.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Startup catalog seed check failed; scheduled nightly sync will retry.");
        }
    }

    private async Task RunScopedSyncAsync(string triggerType, bool waitIfBusy, CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<ExoplanetSyncService>();
            await syncService.SyncAsync(triggerType, waitIfBusy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scheduled {TriggerType} sync encountered an unhandled error.", triggerType);
        }
    }

    public static TimeSpan ComputeDelayUntilNextRunUtc(DateTime utcNow, int targetHourUtc)
    {
        var todayRun = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, targetHourUtc, 0, 0, DateTimeKind.Utc);
        var nextRun = todayRun > utcNow ? todayRun : todayRun.AddDays(1);
        return nextRun - utcNow;
    }
}

