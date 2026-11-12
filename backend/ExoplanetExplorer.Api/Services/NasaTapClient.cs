using System.Net;
using System.Net.Http.Json;
using ExoplanetExplorer.Api.Configuration;
using ExoplanetExplorer.Api.Models;
using Microsoft.Extensions.Options;

namespace ExoplanetExplorer.Api.Services;

/// <summary>
/// Client abstraction for fetching confirmed exoplanet composite records from NASA Exoplanet Archive TAP.
/// </summary>
public interface INasaTapClient
{
    Task<IReadOnlyList<NasaTapRow>> FetchConfirmedPlanetsAsync(CancellationToken ct = default);
}

/// <summary>
/// Typed HTTP client querying NASA Exoplanet Archive's <c>pscomppars</c> table in compressed JSON format.
/// </summary>
public sealed class NasaTapClient(
    HttpClient httpClient,
    IOptions<SyncOptions> options,
    ILogger<NasaTapClient> logger) : INasaTapClient
{
    private const string TapAdqlQuery =
        "select pl_name,hostname,discoverymethod,disc_year,pl_rade,pl_bmasse,pl_orbper,pl_orbsmax,pl_eqt,st_lum,st_teff,st_rad,st_mass,sy_dist " +
        "from pscomppars where pl_name is not null";

    public async Task<IReadOnlyList<NasaTapRow>> FetchConfirmedPlanetsAsync(CancellationToken ct = default)
    {
        var baseUrl = options.Value.BaseUrl.TrimEnd('?');
        var requestUri = $"{baseUrl}?query={Uri.EscapeDataString(TapAdqlQuery)}&format=json";

        const int maxAttempts = 3;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                using var response = await httpClient.GetAsync(
                    requestUri,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct).ConfigureAwait(false);

                if (IsTransientStatus(response.StatusCode) && attempt < maxAttempts)
                {
                    logger.LogWarning(
                        "NASA TAP returned transient status {StatusCode} on attempt {Attempt}/{MaxAttempts}.",
                        (int)response.StatusCode,
                        attempt,
                        maxAttempts);
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct).ConfigureAwait(false);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var rows = await response.Content
                    .ReadFromJsonAsync<List<NasaTapRow>>(cancellationToken: ct)
                    .ConfigureAwait(false);

                return rows ?? [];
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (attempt < maxAttempts && ex is HttpRequestException or TaskCanceledException)
            {
                lastError = ex;
                logger.LogWarning(
                    ex,
                    "NASA TAP request attempt {Attempt}/{MaxAttempts} failed; retrying.",
                    attempt,
                    maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct).ConfigureAwait(false);
            }
        }

        throw new HttpRequestException("Failed to fetch exoplanet catalog from NASA TAP after retries.", lastError);
    }

    private static bool IsTransientStatus(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests ||
        statusCode == HttpStatusCode.RequestTimeout ||
        (int)statusCode >= 500;
}

