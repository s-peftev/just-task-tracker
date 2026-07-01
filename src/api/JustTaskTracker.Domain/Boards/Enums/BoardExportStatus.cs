namespace JustTaskTracker.Domain.Boards.Enums;

public enum BoardExportStatus : byte
{
    /// <summary>
    /// The board is active. No data export has been requested.
    /// </summary>
    None = 0,

    /// <summary>
    /// The board is archived (read-only) and scheduled for export. Waiting for the background worker.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The background worker or Azure Function is currently exporting the board data.
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Export is complete. The exported file has been successfully uploaded to storage.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// The export process failed. Data remains in the primary database for a retry.
    /// </summary>
    Failed = 4
}
