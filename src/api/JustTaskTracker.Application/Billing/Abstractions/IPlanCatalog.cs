using JustTaskTracker.Domain.Billing.DTOs;

namespace JustTaskTracker.Application.Billing.Abstractions;

/// <summary>
/// Read-only view of the billing plan catalog defined in application configuration.
/// </summary>
public interface IPlanCatalog
{
    /// <summary>
    /// Identifier of the plan assigned to users without an active billable subscription.
    /// </summary>
    string DefaultPlanId { get; }

    /// <summary>
    /// Returns the plan definition for <paramref name="planId"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the plan is not defined in configuration.</exception>
    PlanDto GetPlan(string planId);

    /// <summary>
    /// Returns all plans from the catalog.
    /// </summary>
    IReadOnlyList<PlanDto> GetAllPlans();

    /// <summary>
    /// Returns the Stripe price id for <paramref name="planId"/>, or
    /// <see langword="null"/> when the plan has no price configured.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the plan is not defined in configuration.</exception>
    string? TryGetPriceId(string planId);

    /// <summary>
    /// Returns price id configured for <paramref name="planId"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// When the plan is not defined or has no price id configured.
    /// </exception>
    string GetPriceId(string planId);
}
