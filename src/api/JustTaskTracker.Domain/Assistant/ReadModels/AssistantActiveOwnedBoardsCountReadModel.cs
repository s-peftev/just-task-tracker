namespace JustTaskTracker.Domain.Assistant.ReadModels;

/// <summary>
/// Keyless projection of <c>vw_Assistant_ActiveOwnedBoardsCount</c>:
/// number of non-archived boards where the user is Owner.
/// </summary>
public class AssistantActiveOwnedBoardsCountReadModel
{
    public Guid UserId { get; set; }

    public int ActiveOwnedBoardsCount { get; set; }
}
