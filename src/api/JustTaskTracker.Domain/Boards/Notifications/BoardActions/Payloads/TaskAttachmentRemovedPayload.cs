namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskAttachmentRemovedPayload(
    Guid BoardTaskId,
    Guid AttachmentId) : BoardActionPayload;
