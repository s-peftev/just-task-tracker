namespace JustTaskTracker.WebUI.Domain.Boards.Enums;

public enum BoardExportStatus : byte
{
    None = 0,
    Requested = 1,
    Pending = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5,
}
