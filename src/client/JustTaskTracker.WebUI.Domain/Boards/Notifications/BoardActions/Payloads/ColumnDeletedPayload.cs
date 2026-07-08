using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads.Positions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record ColumnDeletedPayload(
    Guid ColumnId,
    DeleteColumnTasksDisposition TasksDisposition,
    Guid? TargetColumnId,
    ColumnTaskMovePlacement? MovePlacement,
    IReadOnlyList<BoardActionColumnPosition> RemainingColumns,
    IReadOnlyList<BoardActionTaskPosition>? MovedTasks) : BoardActionPayload;
