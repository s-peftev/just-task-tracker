namespace JustTaskTracker.Domain.Kanban.Enums;

public enum BoardAccessStatus : byte
{
    NotFound = 0,
    Forbidden = 1,
    Allowed = 2
}
