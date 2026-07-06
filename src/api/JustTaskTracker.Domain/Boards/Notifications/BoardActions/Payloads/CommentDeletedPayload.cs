namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record CommentDeletedPayload(
    Guid BoardTaskId,
    Guid CommentId) : BoardActionPayload;
