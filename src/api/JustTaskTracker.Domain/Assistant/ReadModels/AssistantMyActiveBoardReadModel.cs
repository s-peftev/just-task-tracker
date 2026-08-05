namespace JustTaskTracker.Domain.Assistant.ReadModels;

/// <summary>
/// Keyless projection of <c>vw_Assistant_MyActiveBoards</c>:
/// active (non-archived, non-deleted) boards where the user is a member.
/// </summary>
public class AssistantMyActiveBoardReadModel
{
    public Guid UserId { get; set; }

    public Guid BoardId { get; set; }

    public required string BoardName { get; set; }
}
