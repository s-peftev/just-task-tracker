namespace JustTaskTracker.Domain.Calls.Notifications;

/// <summary>
/// In-call live state changes relayed to clients viewing the board a call belongs to (AD-10).
/// </summary>
public enum CallStateNotificationType : byte
{
    ParticipantJoined = 1,
    ParticipantLeft = 2,
    SessionClosed = 3,
    PresenterChanged = 4,
}
