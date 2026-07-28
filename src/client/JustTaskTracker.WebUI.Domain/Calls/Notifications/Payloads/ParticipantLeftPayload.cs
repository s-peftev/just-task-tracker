using JustTaskTracker.WebUI.Domain.Calls.Notifications;

namespace JustTaskTracker.WebUI.Domain.Calls.Notifications.Payloads;

public record ParticipantLeftPayload(Guid UserId, DateTime LeftAtUtc) : CallStatePayload;
