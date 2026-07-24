namespace JustTaskTracker.Domain.Calls.Enums;

public enum CallStatus : byte
{
    // Room is provisioned and open for participants
    Active = 1,

    // Closed after the last active participant left, or force-ended
    Closed = 2
}
