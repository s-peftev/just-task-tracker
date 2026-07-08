using System.Text.Json;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

public sealed record BoardActionNotificationWireDto(
    Guid BoardId,
    BoardActionNotificationType Type,
    Guid ActorUserId,
    DateTime OccurredAtUtc,
    JsonElement Payload);
