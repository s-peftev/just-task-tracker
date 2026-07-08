namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskAttachmentsCountChangedPayload(
    Guid BoardTaskId,
    int AttachmentsCount) : BoardActionPayload;
