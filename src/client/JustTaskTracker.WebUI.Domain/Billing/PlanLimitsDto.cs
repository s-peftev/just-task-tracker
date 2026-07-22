namespace JustTaskTracker.WebUI.Domain.Billing;

public record PlanLimitsDto(
    int? MaxBoards,
    int? MaxColumnsPerBoard,
    int? MaxTasksPerBoard,
    int? MaxMembersPerBoard);
