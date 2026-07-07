namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

public record BoardActionNotification(
    Guid BoardId,
    BoardActionNotificationType Type,
    Guid ActorUserId,
    DateTime OccurredAtUtc,
    BoardActionPayload Payload);
