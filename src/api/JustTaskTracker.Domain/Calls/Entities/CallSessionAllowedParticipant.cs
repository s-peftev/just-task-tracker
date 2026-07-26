namespace JustTaskTracker.Domain.Calls.Entities;

// Restricted-visibility allow-list (AD-8/AD-4). Only populated when CallSession.Visibility is
// Restricted; the creator is always implicitly allowed regardless of whether they appear here.
public class CallSessionAllowedParticipant
{
    public Guid CallSessionId { get; set; }
    public Guid UserId { get; set; }
}
