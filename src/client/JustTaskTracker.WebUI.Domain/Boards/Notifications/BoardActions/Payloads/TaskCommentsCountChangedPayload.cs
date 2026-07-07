using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskCommentsCountChangedPayload(
    Guid BoardTaskId,
    int CommentsCount) : BoardActionPayload;
