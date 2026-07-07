using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record CommentCreatedPayload(
    Guid BoardTaskId,
    BoardTaskCommentDto Comment) : BoardActionPayload;
