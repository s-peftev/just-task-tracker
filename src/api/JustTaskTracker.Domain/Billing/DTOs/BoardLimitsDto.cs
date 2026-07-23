namespace JustTaskTracker.Domain.Billing.DTOs;

/// <summary>
/// Board-scoped plan limits that apply to a board based on its owner's entitlements.
/// A <see langword="null"/> value means unlimited.
/// </summary>
public record BoardLimitsDto(
    int? MaxColumnsPerBoard,
    int? MaxTasksPerBoard,
    int? MaxMembersPerBoard);
