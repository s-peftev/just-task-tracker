namespace JustTaskTracker.WebUI.Domain.Boards.Enums;

public enum BoardSerializationStatus : byte
{
    None = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
}
