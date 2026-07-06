using JustTaskTracker.Domain.Boards.DTOs.Comments;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record CommentCreatedPayload(
    Guid BoardTaskId,
    BoardTaskCommentDto Comment) : BoardActionPayload;
