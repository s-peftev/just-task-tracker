using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskUpdatedPayload(
    Guid ColumnId,
    Guid BoardTaskId,
    string Title,
    Guid? AssigneeId) : BoardActionPayload;
