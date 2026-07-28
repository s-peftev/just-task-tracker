namespace JustTaskTracker.Domain.Calls.Entities;

// Not a BaseEntity: an append-mostly event-log row (one per join), never soft-deleted.
// JoinedAtUtc/LeftAtUtc are sourced from ACS Event Grid events, never DateTime.UtcNow (AD-12).
public class CallParticipant
{
    public Guid Id { get; set; }
    public Guid CallSessionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    public DateTime? LeftAtUtc { get; set; }
}
