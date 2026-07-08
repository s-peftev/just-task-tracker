using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads.Positions;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record ColumnDeletedPayload(
    Guid ColumnId,
    DeleteColumnTasksDisposition TasksDisposition,
    Guid? TargetColumnId,
    ColumnTaskMovePlacement? MovePlacement,
    IReadOnlyList<BoardActionColumnPosition> RemainingColumns,
    IReadOnlyList<BoardActionTaskPosition>? MovedTasks) : BoardActionPayload;
