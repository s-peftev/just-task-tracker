namespace JustTaskTracker.WebUI.Domain.Billing;

/// <summary>
/// Client mirror of API board-scoped plan limits.
/// A <see langword="null"/> value means unlimited.
/// </summary>
public record BoardLimitsDto(
    int? MaxColumnsPerBoard,
    int? MaxTasksPerBoard,
    int? MaxMembersPerBoard);
