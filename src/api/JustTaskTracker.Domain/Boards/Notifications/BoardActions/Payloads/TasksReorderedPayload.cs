using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads.Positions;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TasksReorderedPayload(
    Guid BoardTaskId,
    Guid SourceColumnId,
    Guid TargetColumnId,
    int Position,
    IReadOnlyList<BoardActionTaskPosition> SourceColumnTasks,
    IReadOnlyList<BoardActionTaskPosition> TargetColumnTasks) : BoardActionPayload;
