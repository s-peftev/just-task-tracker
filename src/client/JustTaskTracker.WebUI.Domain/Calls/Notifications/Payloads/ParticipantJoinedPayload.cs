using JustTaskTracker.WebUI.Domain.Calls.Notifications;

namespace JustTaskTracker.WebUI.Domain.Calls.Notifications.Payloads;

public record ParticipantJoinedPayload(Guid UserId, DateTime JoinedAtUtc) : CallStatePayload;
