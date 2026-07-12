using JustTaskTracker.WebUI.Domain.Billing.Constants;

namespace JustTaskTracker.WebUI.Domain.Billing;

/// <summary>
/// Presentation helpers for subscription status badges on plan cards.
/// Only statuses that should surface in the UI return a label.
/// </summary>
public static class SubscriptionStatusFormatting
{
    public static string? TryGetBadgeLabel(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return status.ToLowerInvariant() switch
        {
            SubscriptionStatuses.Active => "Active",
            SubscriptionStatuses.PastDue => "Past due",
            _ => null,
        };
    }

    public static string? TryGetBadgeCssModifier(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return status.ToLowerInvariant() switch
        {
            SubscriptionStatuses.Active => "plan-card__status--active",
            SubscriptionStatuses.PastDue => "plan-card__status--past-due",
            _ => null,
        };
    }
}
