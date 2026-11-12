using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ExoplanetExplorer.Api.Data;
using ExoplanetExplorer.Api.Data.Entities;
using ExoplanetExplorer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExoplanetExplorer.Api.Services;

/// <summary>
/// Process-wide concurrency coordinator ensuring background and manual syncs never overlap.
/// </summary>
public sealed class SyncCoordinator
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private int _isRunning;

    public bool IsSyncRunning => Volatile.Read(ref _isRunning) == 1;

    public async Task<bool> TryAcquireAsync(bool waitIfBusy, CancellationToken ct)
    {
        if (waitIfBusy)
        {
            await _lock.WaitAsync(ct).ConfigureAwait(false);
            Volatile.Write(ref _isRunning, 1);
            return true;
        }

        if (!await _lock.WaitAsync(0, ct).ConfigureAwait(false))
        {
            return false;
        }

        Volatile.Write(ref _isRunning, 1);
        return true;
    }

    public void Release()
    {
        Volatile.Write(ref _isRunning, 0);
        _lock.Release();
    }
}

public sealed record SyncExecutionResult(
    bool Accepted,
    SyncRunDto? Run,
    string? ConflictReason = null);

/// <summary>
/// Synchronizes confirmed exoplanet data from NASA TAP into SQL Server with hash-based incremental upserts.
/// </summary>
public sealed class ExoplanetSyncService(
    ExoplanetDbContext db,
    INasaTapClient nasaClient,
    SyncCoordinator coordinator,
    CatalogCacheInvalidator cacheInvalidator,
    ILogger<ExoplanetSyncService> logger)
{
    private const int BatchSize = 500;

    public async Task<SyncExecutionResult> SyncAsync(
        string triggerType,
        bool waitIfBusy = false,
        CancellationToken ct = default)
    {
        if (!await coordinator.TryAcquireAsync(waitIfBusy, ct).ConfigureAwait(false))
        {
            return new SyncExecutionResult(false, null, "A NASA catalog synchronization is already in progress.");
        }

        var run = new SyncRun
        {
            StartedUtc = DateTime.UtcNow,
            TriggerType = Truncate(triggerType, 16),
            Status = "Running"
        };

        try
        {
            db.SyncRuns.Add(run);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            var rawRows = await nasaClient.FetchConfirmedPlanetsAsync(ct).ConfigureAwait(false);
            var normalizedRows = NormalizeAndDeduplicate(rawRows);
            run.FetchedCount = normalizedRows.Count;

            if (normalizedRows.Count == 0)
            {
                throw new InvalidOperationException("NASA TAP returned zero valid exoplanet rows; aborting sync to preserve existing catalog.");
            }

            var existingMap = await db.Exoplanets
                .AsNoTracking()
                .Select(e => new { e.Id, e.PlanetName, e.ContentHash })
                .ToDictionaryAsync(e => e.PlanetName, StringComparer.Ordinal, ct)
                .ConfigureAwait(false);

            var nowUtc = DateTime.UtcNow;
            var toInsert = new List<Exoplanet>();
            var toUpdate = new List<Exoplanet>();
            var unchangedCount = 0;

            foreach (var row in normalizedRows)
            {
                var planetName = Truncate(row.PlanetName!.Trim(), 120);
                var contentHash = ComputeContentHash(row);

                if (!existingMap.TryGetValue(planetName, out var existing))
                {
                    toInsert.Add(MapToEntity(0, planetName, row, contentHash, nowUtc));
                }
                else if (!string.Equals(existing.ContentHash, contentHash, StringComparison.Ordinal))
                {
                    toUpdate.Add(MapToEntity(existing.Id, planetName, row, contentHash, nowUtc));
                }
                else
                {
                    unchangedCount++;
                }
            }

            run.InsertedCount = toInsert.Count;
            run.UpdatedCount = toUpdate.Count;
            run.UnchangedCount = unchangedCount;

            await PersistChangesInBatchesAsync(toInsert, toUpdate, ct).ConfigureAwait(false);

            run.Status = "Succeeded";
            run.CompletedUtc = DateTime.UtcNow;

            db.Attach(run);
            db.Entry(run).State = EntityState.Modified;
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            if (toInsert.Count > 0 || toUpdate.Count > 0)
            {
                cacheInvalidator.AdvanceEpoch();
            }

            logger.LogInformation(
                "NASA catalog sync ({TriggerType}) succeeded: fetched={Fetched}, inserted={Inserted}, updated={Updated}, unchanged={Unchanged}.",
                triggerType,
                run.FetchedCount,
                run.InsertedCount,
                run.UpdatedCount,
                run.UnchangedCount);

            return new SyncExecutionResult(true, ToDto(run));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await MarkRunFailedSafeAsync(run, "Synchronization canceled during shutdown.").ConfigureAwait(false);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NASA catalog sync ({TriggerType}) failed.", triggerType);
            await MarkRunFailedSafeAsync(run, ex.Message).ConfigureAwait(false);
            return new SyncExecutionResult(true, ToDto(run));
        }
        finally
        {
            coordinator.Release();
        }
    }

    private async Task PersistChangesInBatchesAsync(
        List<Exoplanet> toInsert,
        List<Exoplanet> toUpdate,
        CancellationToken ct)
    {
        if (toInsert.Count == 0 && toUpdate.Count == 0)
        {
            return;
        }

        var useTransaction = db.Database.IsRelational();
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            var originalAutoDetect = db.ChangeTracker.AutoDetectChangesEnabled;
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                db.ChangeTracker.Clear();

                // Reset keys on retry in case a rolled-back batch assigned identity values on entities.
                foreach (var entity in toInsert)
                {
                    entity.Id = 0;
                }

                await using var transaction = useTransaction
                    ? await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false)
                    : null;

                for (var i = 0; i < toInsert.Count; i += BatchSize)
                {
                    var slice = toInsert.GetRange(i, Math.Min(BatchSize, toInsert.Count - i));
                    db.Exoplanets.AddRange(slice);
                    await db.SaveChangesAsync(ct).ConfigureAwait(false);
                    db.ChangeTracker.Clear();
                }

                for (var i = 0; i < toUpdate.Count; i += BatchSize)
                {
                    var slice = toUpdate.GetRange(i, Math.Min(BatchSize, toUpdate.Count - i));
                    db.Exoplanets.UpdateRange(slice);
                    await db.SaveChangesAsync(ct).ConfigureAwait(false);
                    db.ChangeTracker.Clear();
                }

                if (transaction is not null)
                {
                    await transaction.CommitAsync(ct).ConfigureAwait(false);
                }
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            }
        }).ConfigureAwait(false);
    }

    private async Task MarkRunFailedSafeAsync(SyncRun run, string message)
    {
        try
        {
            db.ChangeTracker.Clear();
            run.Status = "Failed";
            run.CompletedUtc = DateTime.UtcNow;
            run.ErrorMessage = Truncate(message, 1000);

            if (run.Id == 0)
            {
                db.SyncRuns.Add(run);
            }
            else
            {
                db.SyncRuns.Update(run);
            }

            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception logEx)
        {
            logger.LogError(logEx, "Failed to persist SyncRun failure status.");
        }
    }

    private static List<NasaTapRow> NormalizeAndDeduplicate(IReadOnlyList<NasaTapRow> rawRows)
    {
        var byName = new Dictionary<string, NasaTapRow>(rawRows.Count, StringComparer.Ordinal);
        foreach (var row in rawRows)
        {
            if (string.IsNullOrWhiteSpace(row.PlanetName))
            {
                continue;
            }

            var key = Truncate(row.PlanetName.Trim(), 120);
            byName[key] = row;
        }

        return [.. byName.Values];
    }

    private static Exoplanet MapToEntity(
        int existingId,
        string planetName,
        NasaTapRow row,
        string contentHash,
        DateTime syncedUtc)
    {
        var hz = HabitableZoneCalculator.Evaluate(new HabitableZoneInput(
            row.StellarLuminosityLogSolar,
            row.StellarTeffK,
            row.StellarRadiusSolar,
            row.StellarMassSolar,
            row.SemiMajorAxisAu,
            row.OrbitalPeriodDays,
            row.RadiusEarth,
            row.EquilibriumTempK));

        return new Exoplanet
        {
            Id = existingId,
            PlanetName = planetName,
            HostName = Truncate(string.IsNullOrWhiteSpace(row.HostName) ? "Unknown" : row.HostName.Trim(), 100),
            DiscoveryMethod = Truncate(string.IsNullOrWhiteSpace(row.DiscoveryMethod) ? "Unknown" : row.DiscoveryMethod.Trim(), 64),
            DiscoveryYear = row.DiscoveryYear is >= 1980 and <= 2100 ? row.DiscoveryYear : null,
            MassEarth = HabitableZoneCalculator.FinitePositive(row.MassEarth),
            RadiusEarth = HabitableZoneCalculator.FinitePositive(row.RadiusEarth),
            OrbitalPeriodDays = HabitableZoneCalculator.FinitePositive(row.OrbitalPeriodDays),
            SemiMajorAxisAu = hz.SemiMajorAxisAu,
            SemiMajorAxisDerived = hz.SemiMajorAxisDerived,
            EquilibriumTempK = HabitableZoneCalculator.FinitePositive(row.EquilibriumTempK),
            StellarLuminosityLogSolar = FiniteOrNull(row.StellarLuminosityLogSolar),
            StellarLuminositySolar = hz.StellarLuminositySolar,
            LuminosityDerived = hz.LuminosityDerived,
            StellarTeffK = HabitableZoneCalculator.FinitePositive(row.StellarTeffK),
            StellarRadiusSolar = HabitableZoneCalculator.FinitePositive(row.StellarRadiusSolar),
            StellarMassSolar = HabitableZoneCalculator.FinitePositive(row.StellarMassSolar),
            DistanceParsecs = HabitableZoneCalculator.FinitePositive(row.DistanceParsecs),
            HzInnerConservativeAu = hz.HzInnerConservativeAu,
            HzOuterConservativeAu = hz.HzOuterConservativeAu,
            HzInnerOptimisticAu = hz.HzInnerOptimisticAu,
            HzOuterOptimisticAu = hz.HzOuterOptimisticAu,
            InsolationEarth = hz.InsolationEarth,
            HabitabilityStatus = hz.HabitabilityStatus,
            IsRockyCandidate = hz.IsRockyCandidate,
            EarthSimilarityScore = hz.EarthSimilarityScore,
            ContentHash = contentHash,
            LastSyncedUtc = syncedUtc
        };
    }

    public static string ComputeContentHash(NasaTapRow row)
    {
        var canonical = string.Join('|',
            HabitableZoneCalculator.FormulaVersion,
            row.PlanetName?.Trim() ?? string.Empty,
            row.HostName?.Trim() ?? string.Empty,
            row.DiscoveryMethod?.Trim() ?? string.Empty,
            row.DiscoveryYear?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            FormatDouble(row.RadiusEarth),
            FormatDouble(row.MassEarth),
            FormatDouble(row.OrbitalPeriodDays),
            FormatDouble(row.SemiMajorAxisAu),
            FormatDouble(row.EquilibriumTempK),
            FormatDouble(row.StellarLuminosityLogSolar),
            FormatDouble(row.StellarTeffK),
            FormatDouble(row.StellarRadiusSolar),
            FormatDouble(row.StellarMassSolar),
            FormatDouble(row.DistanceParsecs));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(bytes);
    }

    private static string FormatDouble(double? value) =>
        value is double v && double.IsFinite(v)
            ? v.ToString("G12", CultureInfo.InvariantCulture)
            : string.Empty;

    private static double? FiniteOrNull(double? value) =>
        value is double v && double.IsFinite(v) ? v : null;

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    public static SyncRunDto ToDto(SyncRun run) =>
        new(
            run.Id,
            run.StartedUtc,
            run.CompletedUtc,
            run.TriggerType,
            run.Status,
            run.FetchedCount,
            run.InsertedCount,
            run.UpdatedCount,
            run.UnchangedCount,
            run.ErrorMessage);
}
