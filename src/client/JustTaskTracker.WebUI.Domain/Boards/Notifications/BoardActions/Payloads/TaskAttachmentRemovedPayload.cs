using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskAttachmentRemovedPayload(
    Guid BoardTaskId,
    Guid AttachmentId) : BoardActionPayload;
