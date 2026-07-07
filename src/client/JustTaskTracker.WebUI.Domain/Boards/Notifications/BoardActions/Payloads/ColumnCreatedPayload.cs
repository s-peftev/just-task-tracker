using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record ColumnCreatedPayload(
    Guid ColumnId,
    string Name,
    int Position) : BoardActionPayload;
