namespace JustTaskTracker.Domain.Billing.DTOs;

/// <summary>
/// Normalized Stripe webhook payload used by the application to sync subscription state.
/// </summary>
public record BillingWebhookEvent(
    string EventId,
    string EventType,
    string? StripeCustomerId,
    string? StripeSubscriptionId,
    string? StripePriceId,
    string? PlanId,
    string? Status,
    DateTime? CurrentPeriodStartUtc,
    DateTime? CurrentPeriodEndUtc,
    bool CancelAtPeriodEnd,
    Guid? UserId);
