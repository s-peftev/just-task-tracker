using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record AttachmentUploadedPayload(
    Guid BoardTaskId,
    BoardTaskAttachmentDto Attachment) : BoardActionPayload;
