using JustTaskTracker.WebUI.Domain.Billing.Constants;

namespace JustTaskTracker.WebUI.Domain.Billing;

/// <summary>
/// Resolves optional status-note copy for a plan card from <see cref="SubscriptionDetailsDto"/>.
/// Returns <see langword="null"/> when nothing should be shown (free / other card / unsupported status).
/// </summary>
public static class PlanCardSubscriptionNoticeResolver
{
    public static PlanCardSubscriptionNotice? Resolve(
        string planId,
        SubscriptionDetailsDto? subscription,
        DateTime utcNow)
    {
        if (subscription is null || !subscription.HasBillableSubscription)
            return null;

        if (!planId.Equals(subscription.PlanId, StringComparison.OrdinalIgnoreCase))
            return null;

        return subscription.Status.ToLowerInvariant() switch
        {
            SubscriptionStatuses.Active => ResolveActive(subscription, utcNow),
            SubscriptionStatuses.PastDue => new PlanCardSubscriptionNotice(
                PlanCardSubscriptionNoticeKind.PastDue,
                "Payment issue. Update your payment details in the billing portal today so your subscription is not canceled tomorrow."),
            _ => null,
        };
    }

    private static PlanCardSubscriptionNotice? ResolveActive(
        SubscriptionDetailsDto subscription,
        DateTime utcNow)
    {
        if (subscription.CurrentPeriodEndUtc is not { } periodEndUtc)
            return null;

        var daysLeft = GetDaysRemaining(periodEndUtc, utcNow);
        var dateText = FormatPeriodEnd(periodEndUtc);
        var daysText = FormatDaysLeft(daysLeft);

        if (subscription.CancelAtPeriodEnd)
        {
            return new PlanCardSubscriptionNotice(
                PlanCardSubscriptionNoticeKind.Cancels,
                $"Cancels on {dateText} ({daysText}).");
        }

        return new PlanCardSubscriptionNotice(
            PlanCardSubscriptionNoticeKind.Renews,
            $"Renews on {dateText} ({daysText}).");
    }

    private static int GetDaysRemaining(DateTime periodEndUtc, DateTime utcNow)
    {
        var endUtc = AsUtc(periodEndUtc);
        var nowUtc = AsUtc(utcNow);
        var days = (int)Math.Ceiling((endUtc - nowUtc).TotalDays);

        return Math.Max(0, days);
    }

    private static string FormatPeriodEnd(DateTime periodEndUtc) =>
        AsUtc(periodEndUtc).ToLocalTime().ToString("MMM d, yyyy");

    private static string FormatDaysLeft(int days) =>
        days == 1 ? "1 day left" : $"{days} days left";

    private static DateTime AsUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
}
