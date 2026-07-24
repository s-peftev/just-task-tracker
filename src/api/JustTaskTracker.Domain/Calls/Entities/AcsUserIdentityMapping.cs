namespace JustTaskTracker.Domain.Calls.Entities;

// Not a BaseEntity: a lightweight lookup row (UserId -> ACS identity), no audit/soft-delete needed.
// Same shape convention as BoardMember (a mapping keyed by its own natural key, not a Guid Id).
public class AcsUserIdentityMapping
{
    public Guid UserId { get; set; }
    public string AcsCommunicationUserId { get; set; } = string.Empty;
}
