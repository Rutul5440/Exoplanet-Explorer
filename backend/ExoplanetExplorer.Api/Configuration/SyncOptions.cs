using System.ComponentModel.DataAnnotations;

namespace ExoplanetExplorer.Api.Configuration;

/// <summary>
/// Configuration options governing NASA Exoplanet Archive TAP synchronization and scheduling.
/// </summary>
public sealed class SyncOptions
{
    public const string SectionName = "Sync";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://exoplanetarchive.ipac.caltech.edu/TAP/sync";

    [Range(0, 23)]
    public int DailyRunHourUtc { get; set; } = 2;

    [Range(10, 600)]
    public int SyncTimeoutSeconds { get; set; } = 90;

    public bool EnableStartupSeed { get; set; } = true;

    public bool EnableBackgroundTimer { get; set; } = true;
}

