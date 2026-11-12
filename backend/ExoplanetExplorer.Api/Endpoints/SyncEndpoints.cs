using ExoplanetExplorer.Api.Data;
using ExoplanetExplorer.Api.Services;

namespace ExoplanetExplorer.Api.Endpoints;

/// <summary>
/// Minimal API route mappings for NASA archive synchronization telemetry, manual triggers, and health checks.
/// </summary>
public static class SyncEndpoints
{
    public static IEndpointRouteBuilder MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api");

        group.MapGet("/sync/status", async (
            ExoplanetQueryService queryService,
            SyncCoordinator coordinator,
            CancellationToken ct) =>
        {
            var status = await queryService.GetSyncStatusAsync(coordinator.IsSyncRunning, ct);
            return Results.Ok(status);
        })
        .WithName("GetSyncStatus")
        .WithOpenApi();

        group.MapPost("/sync/trigger", async (
            ExoplanetSyncService syncService,
            CancellationToken ct) =>
        {
            var result = await syncService.SyncAsync("Manual", waitIfBusy: false, ct);
            if (!result.Accepted)
            {
                return Results.Problem(result.ConflictReason, statusCode: StatusCodes.Status409Conflict);
            }

            if (result.Run?.Status == "Failed")
            {
                return Results.Problem(
                    result.Run.ErrorMessage ?? "NASA catalog synchronization failed.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            return Results.Ok(result.Run);
        })
        .WithName("TriggerSync")
        .WithOpenApi();

        group.MapGet("/health", async (ExoplanetDbContext db, CancellationToken ct) =>
        {
            var canConnect = await db.Database.CanConnectAsync(ct);
            return canConnect
                ? Results.Ok(new { status = "healthy", database = "connected" })
                : Results.Problem("Database unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .WithName("GetHealth")
        .WithOpenApi();

        return app;
    }
}

