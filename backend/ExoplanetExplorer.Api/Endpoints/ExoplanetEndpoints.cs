using ExoplanetExplorer.Api.Models;
using ExoplanetExplorer.Api.Services;

namespace ExoplanetExplorer.Api.Endpoints;

/// <summary>
/// Minimal API route mappings for browsing, filtering, charting, and comparing exoplanets.
/// </summary>
public static class ExoplanetEndpoints
{
    public static IEndpointRouteBuilder MapExoplanetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api");

        group.MapGet("/exoplanets", async (
            [AsParameters] ExoplanetQueryParameters query,
            ExoplanetQueryService service,
            CancellationToken ct) =>
        {
            var validationError = ExoplanetQueryService.ValidateFilters(query);
            if (validationError is not null)
            {
                return Results.Problem(validationError, statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await service.GetPagedAsync(query, ct);
            return Results.Ok(result);
        })
        .WithName("GetExoplanets")
        .WithOpenApi();

        group.MapGet("/exoplanets/compare", async (
            string? ids,
            ExoplanetQueryService service,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return Results.Problem("Query parameter 'ids' is required (e.g. ?ids=1,2).", statusCode: StatusCodes.Status400BadRequest);
            }

            var parsedIds = new List<int>();
            foreach (var token in ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!int.TryParse(token, out var id))
                {
                    return Results.Problem($"Invalid planet id '{token}'.", statusCode: StatusCodes.Status400BadRequest);
                }

                parsedIds.Add(id);
            }

            var (response, error) = await service.CompareAsync(parsedIds, ct);
            if (error is not null)
            {
                return Results.Problem(
                    error,
                    statusCode: response is null && error.Contains("found", StringComparison.OrdinalIgnoreCase)
                        ? StatusCodes.Status404NotFound
                        : StatusCodes.Status400BadRequest);
            }

            return Results.Ok(response);
        })
        .WithName("CompareExoplanets")
        .WithOpenApi();

        group.MapGet("/exoplanets/{id:int}", async (
            int id,
            ExoplanetQueryService service,
            CancellationToken ct) =>
        {
            var detail = await service.GetByIdAsync(id, ct);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .WithName("GetExoplanetById")
        .WithOpenApi();

        group.MapGet("/charts/mass-radius", async (
            [AsParameters] ExoplanetQueryParameters query,
            ExoplanetQueryService service,
            CancellationToken ct) =>
        {
            var validationError = ExoplanetQueryService.ValidateFilters(query);
            if (validationError is not null)
            {
                return Results.Problem(validationError, statusCode: StatusCodes.Status400BadRequest);
            }

            var data = await service.GetMassRadiusChartAsync(query, ct);
            return Results.Ok(data);
        })
        .WithName("GetMassRadiusChart")
        .WithOpenApi();

        group.MapGet("/charts/discovery-trends", async (
            [AsParameters] ExoplanetQueryParameters query,
            ExoplanetQueryService service,
            CancellationToken ct) =>
        {
            var validationError = ExoplanetQueryService.ValidateFilters(query);
            if (validationError is not null)
            {
                return Results.Problem(validationError, statusCode: StatusCodes.Status400BadRequest);
            }

            var data = await service.GetDiscoveryTrendsAsync(query, ct);
            return Results.Ok(data);
        })
        .WithName("GetDiscoveryTrendsChart")
        .WithOpenApi();

        group.MapGet("/stats/summary", async (
            ExoplanetQueryService service,
            CancellationToken ct) =>
        {
            var summary = await service.GetSummaryAsync(ct);
            return Results.Ok(summary);
        })
        .WithName("GetCatalogSummary")
        .WithOpenApi();

        return app;
    }
}

