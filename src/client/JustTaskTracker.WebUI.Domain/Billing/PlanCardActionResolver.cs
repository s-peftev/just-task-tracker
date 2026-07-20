using JustTaskTracker.WebUI.Domain.Billing.Constants;

namespace JustTaskTracker.WebUI.Domain.Billing;

public enum PlanCardActionKind
{
    Current,
    Upgrade,
    ReturnToFree,
}

public sealed record PlanCardAction(
    PlanCardActionKind Kind,
    string Label,
    bool IsEnabled);

/// <summary>
/// Resolves the CTA under a plan card from the user's current subscription.
/// </summary>
public static class PlanCardActionResolver
{
    public static PlanCardAction? Resolve(
        PlanCardDto plan,
        SubscriptionDetailsDto? subscription)
    {
        // Free has no CTA: current-plan / return-to-free buttons are intentionally omitted.
        if (IsFreePlan(plan.PlanId))
            return null;

        var currentPlanId = subscription?.PlanId;

        if (IsSamePlan(plan.PlanId, currentPlanId))
        {
            return new PlanCardAction(
                PlanCardActionKind.Current,
                "Your Current Plan",
                IsEnabled: false);
        }

        // Paid plan card: upgrade only while the user is on Free.
        if (IsOnFree(currentPlanId))
        {
            return new PlanCardAction(
                PlanCardActionKind.Upgrade,
                $"Upgrade to {plan.PlanDisplayName}",
                IsEnabled: true);
        }

        return null;
    }

    private static bool IsSamePlan(string planId, string? currentPlanId) =>
        !string.IsNullOrWhiteSpace(currentPlanId)
        && planId.Equals(currentPlanId, StringComparison.OrdinalIgnoreCase);

    private static bool IsFreePlan(string planId) =>
        planId.Equals(PlanIds.Free, StringComparison.OrdinalIgnoreCase);

    private static bool IsOnFree(string? currentPlanId) =>
        string.IsNullOrWhiteSpace(currentPlanId)
        || currentPlanId.Equals(PlanIds.Free, StringComparison.OrdinalIgnoreCase);
}
