namespace JustTaskTracker.Domain.Calls.Constants;

public static class AcsEventGridEventTypes
{
    public const string CallParticipantAdded = "Microsoft.Communication.CallParticipantAdded";
    public const string CallParticipantRemoved = "Microsoft.Communication.CallParticipantRemoved";
    public const string CallEnded = "Microsoft.Communication.CallEnded";
}
