using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads.Positions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record ColumnsReorderedPayload(
    IReadOnlyList<BoardActionColumnPosition> Columns) : BoardActionPayload;
