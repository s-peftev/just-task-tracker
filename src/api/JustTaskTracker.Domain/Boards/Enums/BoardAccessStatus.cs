namespace JustTaskTracker.Domain.Boards.Enums;

public enum BoardAccessStatus : byte
{
    NotFound = 0,
    Forbidden = 1,
    Allowed = 2
}
