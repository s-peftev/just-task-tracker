using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record ColumnRenamedPayload(
    Guid ColumnId,
    string Name) : BoardActionPayload;
