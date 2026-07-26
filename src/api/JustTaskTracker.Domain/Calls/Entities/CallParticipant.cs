namespace JustTaskTracker.Domain.Calls.Entities;

// Stub for Story 1.4 (participant tracking via Event Grid, AD-12). Not yet EF-mapped or persisted.
public class CallParticipant
{
    public Guid Id { get; set; }
    public Guid CallSessionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    public DateTime? LeftAtUtc { get; set; }
}
