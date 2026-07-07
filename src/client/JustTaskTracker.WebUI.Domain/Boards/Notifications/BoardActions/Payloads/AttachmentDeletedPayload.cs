using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record AttachmentDeletedPayload(
    Guid BoardTaskId,
    Guid AttachmentId) : BoardActionPayload;
