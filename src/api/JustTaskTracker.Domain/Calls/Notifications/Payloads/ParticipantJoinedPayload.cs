namespace JustTaskTracker.Domain.Calls.Notifications.Payloads;

public record ParticipantJoinedPayload(Guid UserId, DateTime JoinedAtUtc) : CallStatePayload;
