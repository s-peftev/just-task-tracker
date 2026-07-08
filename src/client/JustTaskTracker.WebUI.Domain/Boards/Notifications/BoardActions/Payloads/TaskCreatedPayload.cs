using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskCreatedPayload(
    Guid ColumnId,
    Guid BoardTaskId,
    string Title,
    int Position,
    Guid? AssigneeId) : BoardActionPayload;
