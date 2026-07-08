namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskCommentsCountChangedPayload(
    Guid BoardTaskId,
    int CommentsCount) : BoardActionPayload;
