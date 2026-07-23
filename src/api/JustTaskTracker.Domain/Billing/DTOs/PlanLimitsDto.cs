namespace JustTaskTracker.Domain.Billing.DTOs;

public record PlanLimitsDto(
    int? MaxBoards,
    int? MaxColumnsPerBoard,
    int? MaxTasksPerBoard,
    int? MaxMembersPerBoard)
{
    public BoardLimitsDto ToBoardLimits() =>
        new(MaxColumnsPerBoard, MaxTasksPerBoard, MaxMembersPerBoard);
}
