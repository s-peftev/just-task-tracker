namespace JustTaskTracker.Domain.Assistant.ReadModels;

/// <summary>
/// Keyless projection of <c>vw_Assistant_MyBoards</c>:
/// non-deleted boards where the user is a member (active and archived).
/// </summary>
public class AssistantMyBoardReadModel
{
    public Guid UserId { get; set; }

    public Guid BoardId { get; set; }

    public required string BoardName { get; set; }

    public bool IsArchived { get; set; }
}
