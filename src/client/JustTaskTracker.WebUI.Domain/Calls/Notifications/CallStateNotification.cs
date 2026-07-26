namespace JustTaskTracker.WebUI.Domain.Calls.Notifications;

public record CallStateNotification(
    Guid BoardId,
    Guid CallSessionId,
    CallStateNotificationType Type,
    CallStatePayload Payload);
