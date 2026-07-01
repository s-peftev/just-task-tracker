namespace JustTaskTracker.Archival.Functions.Contracts.Enums;

public enum BoardExportStatus : byte
{
    /// <summary>
    /// The board is active. No data export has been requested.
    /// </summary>
    None = 0,

    /// <summary>
    /// Export has been requested and is waiting to be scheduled.
    /// </summary>
    Requested = 1,

    /// <summary>
    /// The board is archived (read-only) and scheduled for export. Waiting for the background worker.
    /// </summary>
    Pending = 2,

    /// <summary>
    /// The background worker or Azure Function is currently exporting the board data.
    /// </summary>
    Processing = 3,

    /// <summary>
    /// Export is complete. The exported file has been successfully uploaded to storage.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// The export process failed. Data remains in the primary database for a retry.
    /// </summary>
    Failed = 5
}
