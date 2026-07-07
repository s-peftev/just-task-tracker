using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskAttachmentsCountChangedPayload(
    Guid BoardTaskId,
    int AttachmentsCount) : BoardActionPayload;
