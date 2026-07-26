namespace JustTaskTracker.Domain.Calls.Enums;

public enum CallVisibility : byte
{
    // Any board member may join
    Open = 1,

    // Only users in CallSessionAllowedParticipant (plus Owner/Admin) may join
    Restricted = 2
}
