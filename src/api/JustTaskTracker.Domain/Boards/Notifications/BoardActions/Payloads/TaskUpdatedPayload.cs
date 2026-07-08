namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskUpdatedPayload(
    Guid ColumnId,
    Guid BoardTaskId,
    string Title,
    Guid? AssigneeId) : BoardActionPayload;
