using JustTaskTracker.Domain.Boards.DTOs.Attachments;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskAttachmentAddedPayload(
    Guid BoardTaskId,
    BoardTaskAttachmentDto Attachment) : BoardActionPayload;
