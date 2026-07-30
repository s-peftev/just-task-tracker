namespace JustTaskTracker.Domain.Calls.Notifications.Payloads;

public record PresenterChangedPayload(Guid? PresenterUserId) : CallStatePayload;
