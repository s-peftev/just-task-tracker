namespace JustTaskTracker.Domain.Assistant.ReadModels;

/// <summary>
/// Keyless projection of <c>vw_Assistant_AssignedTasks</c>:
/// tasks assigned to the user on an active board they belong to.
/// </summary>
public class AssistantAssignedTaskReadModel
{
    public Guid UserId { get; set; }

    public Guid BoardId { get; set; }

    public Guid TaskId { get; set; }

    public required string Title { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
