namespace ExoplanetExplorer.Api.Data.Entities;

/// <summary>
/// Audit trail entry for each NASA Exoplanet Archive background or manual sync attempt.
/// </summary>
public sealed class SyncRun
{
    public int Id { get; set; }

    public DateTime StartedUtc { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public string TriggerType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int FetchedCount { get; set; }

    public int InsertedCount { get; set; }

    public int UpdatedCount { get; set; }

    public int UnchangedCount { get; set; }

    public string? ErrorMessage { get; set; }
}

