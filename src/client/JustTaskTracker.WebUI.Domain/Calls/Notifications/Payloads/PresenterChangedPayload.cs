namespace JustTaskTracker.WebUI.Domain.Calls.Notifications.Payloads;

public record PresenterChangedPayload(Guid? PresenterUserId) : CallStatePayload;
