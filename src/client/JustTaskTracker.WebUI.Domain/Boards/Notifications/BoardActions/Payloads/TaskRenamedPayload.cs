using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskRenamedPayload(
    Guid ColumnId,
    Guid BoardTaskId,
    string Title,
    string? Description,
    Guid? AssigneeId) : BoardActionPayload;
