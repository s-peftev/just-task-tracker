namespace JustTaskTracker.Domain.Billing.Enums;

/// <summary>
/// Identifies which plan limit to enforce on a create/add command.
/// </summary>
public enum PlanLimitKind : byte
{
    Boards = 1,
    ColumnsPerBoard = 2,
    TasksPerBoard = 3,
    MembersPerBoard = 4
}
