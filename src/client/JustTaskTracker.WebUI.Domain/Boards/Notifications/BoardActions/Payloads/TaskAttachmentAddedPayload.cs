using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskAttachmentAddedPayload(
    Guid BoardTaskId,
    BoardTaskAttachmentDto Attachment) : BoardActionPayload;
