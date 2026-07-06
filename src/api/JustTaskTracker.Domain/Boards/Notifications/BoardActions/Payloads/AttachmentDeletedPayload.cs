namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record AttachmentDeletedPayload(
    Guid BoardTaskId,
    Guid AttachmentId) : BoardActionPayload;
