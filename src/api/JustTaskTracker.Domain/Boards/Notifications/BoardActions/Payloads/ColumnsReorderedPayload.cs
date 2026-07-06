using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads.Positions;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record ColumnsReorderedPayload(
    IReadOnlyList<BoardActionColumnPosition> Columns) : BoardActionPayload;
