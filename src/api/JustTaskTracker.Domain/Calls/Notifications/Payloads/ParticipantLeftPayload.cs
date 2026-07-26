namespace JustTaskTracker.Domain.Calls.Notifications.Payloads;

public record ParticipantLeftPayload(Guid UserId, DateTime LeftAtUtc) : CallStatePayload;
