using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads.Positions;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskDeletedPayload(
    Guid ColumnId,
    Guid BoardTaskId,
    IReadOnlyList<BoardActionTaskPosition> RemainingTasks) : BoardActionPayload;
