using JustTaskTracker.Domain.Billing.Enums;

namespace JustTaskTracker.Application.Common.Behaviors;

/// <summary>
/// Marks a MediatR request that must stay within the effective plan limit
/// before the handler runs.
/// </summary>
/// <remarks>
/// <see cref="PlanLimitKind.Boards"/> is evaluated against the actor's plan.
/// Board-scoped limits use the board owner's plan.
/// </remarks>
public interface IRequirePlanLimit
{
    PlanLimitKind Limit { get; }

    /// <summary>
    /// Target board for board-scoped limits; <see langword="null"/> for <see cref="PlanLimitKind.Boards"/>.
    /// </summary>
    Guid? BoardId { get; }
}
