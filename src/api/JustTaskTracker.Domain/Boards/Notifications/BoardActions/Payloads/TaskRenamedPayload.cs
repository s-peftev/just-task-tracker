namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskRenamedPayload(
    Guid ColumnId,
    Guid BoardTaskId,
    string Title,
    string? Description,
    Guid? AssigneeId) : BoardActionPayload;
