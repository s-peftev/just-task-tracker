namespace JustTaskTracker.Domain.Assistant.ReadModels;

/// <summary>
/// Keyless projection of <c>vw_Assistant_BoardContext</c>:
/// membership, archive state, owner, and usage counts for a board the user belongs to.
/// </summary>
public class AssistantBoardContextReadModel
{
    public Guid UserId { get; set; }

    public Guid BoardId { get; set; }

    public required string BoardName { get; set; }

    public bool IsArchived { get; set; }

    public byte MemberRole { get; set; }

    public Guid OwnerUserId { get; set; }

    public int ColumnCount { get; set; }

    public int TaskCount { get; set; }

    public int MemberCount { get; set; }
}
