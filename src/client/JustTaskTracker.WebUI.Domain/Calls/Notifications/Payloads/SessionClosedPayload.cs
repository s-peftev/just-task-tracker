using JustTaskTracker.WebUI.Domain.Calls.Notifications;

namespace JustTaskTracker.WebUI.Domain.Calls.Notifications.Payloads;

public record SessionClosedPayload(DateTime EndedAtUtc) : CallStatePayload;
