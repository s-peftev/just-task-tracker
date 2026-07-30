using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskDescriptionChangedPayload(
    Guid BoardTaskId,
    string? Description) : BoardActionPayload;
