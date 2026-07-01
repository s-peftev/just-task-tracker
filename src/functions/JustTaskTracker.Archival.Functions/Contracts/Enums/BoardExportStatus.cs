namespace JustTaskTracker.Archival.Functions.Contracts.Enums;

public enum BoardExportStatus : byte
{
    /// <summary>
    /// The board is active. No data serialization or export has been requested.
    /// </summary>
    None = 0,

    /// <summary>
    /// The board is archived (read-only) and scheduled for serialization. Waiting for the background worker.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The background worker or Azure Function is currently serializing the board data and exporting it.
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Serialization is complete. The exported file has been successfully uploaded to storage.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// The serialization or export process failed. Data remains in the primary database for a retry.
    /// </summary>
    Failed = 4
}
