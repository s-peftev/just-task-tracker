using JustTaskTracker.Domain.Boards.DTOs.Attachments;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record AttachmentUploadedPayload(
    Guid BoardTaskId,
    BoardTaskAttachmentDto Attachment) : BoardActionPayload;
