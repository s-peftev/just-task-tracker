namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads.Positions;

public record BoardActionTaskPosition(Guid BoardTaskId, Guid ColumnId, int Position);
