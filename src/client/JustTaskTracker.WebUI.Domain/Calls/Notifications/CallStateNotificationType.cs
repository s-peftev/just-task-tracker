namespace JustTaskTracker.WebUI.Domain.Calls.Notifications;

public enum CallStateNotificationType : byte
{
    ParticipantJoined = 1,
    ParticipantLeft = 2,
    SessionClosed = 3,
    PresenterChanged = 4,
}
