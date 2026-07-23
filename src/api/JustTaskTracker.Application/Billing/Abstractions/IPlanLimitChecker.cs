using JustTaskTracker.Domain.Billing.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Application.Billing.Abstractions;

/// <summary>
/// Evaluates whether a create/add action is allowed under the effective plan limits.
/// </summary>
public interface IPlanLimitChecker
{
    /// <summary>
    /// Returns <see langword="null"/> when the action is allowed; otherwise the entitlement error to return.
    /// </summary>
    /// <remarks>
    /// <see cref="PlanLimitKind.Boards"/> uses <paramref name="actorUserId"/> entitlements.
    /// Board-scoped kinds use the board owner's entitlements.
    /// A <see langword="null"/> max on the plan means unlimited.
    /// </remarks>
    Task<Error?> EvaluateAsync(
        PlanLimitKind limit,
        Guid actorUserId,
        Guid? boardId,
        CancellationToken ct = default);
}
