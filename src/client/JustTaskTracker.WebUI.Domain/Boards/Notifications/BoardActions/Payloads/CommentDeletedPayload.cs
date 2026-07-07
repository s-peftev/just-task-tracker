using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record CommentDeletedPayload(
    Guid BoardTaskId,
    Guid CommentId) : BoardActionPayload;
