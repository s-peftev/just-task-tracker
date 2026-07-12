namespace JustTaskTracker.WebUI.Domain.Billing.Constants;

/// <summary>
/// Stripe-aligned subscription status values used by the billing API.
/// </summary>
public static class SubscriptionStatuses
{
    public const string Active = "active";
    public const string PastDue = "past_due";
    public const string Trialing = "trialing";
    public const string Canceled = "canceled";
}
