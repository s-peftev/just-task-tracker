namespace JustTaskTracker.Domain.Calls.Notifications.Payloads;

public record SessionClosedPayload(DateTime EndedAtUtc) : CallStatePayload;
