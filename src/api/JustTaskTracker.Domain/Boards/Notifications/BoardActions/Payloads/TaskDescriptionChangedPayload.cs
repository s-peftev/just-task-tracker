namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskDescriptionChangedPayload(
    Guid BoardTaskId,
    string? Description) : BoardActionPayload;
