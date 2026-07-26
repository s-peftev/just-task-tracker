namespace JustTaskTracker.Domain.Calls.Entities;

// Stub for Story 1.2 (Restricted visibility allow-list, AD-8/AD-4). Not yet EF-mapped or persisted.
public class CallSessionAllowedParticipant
{
    public Guid CallSessionId { get; set; }
    public Guid UserId { get; set; }
}
