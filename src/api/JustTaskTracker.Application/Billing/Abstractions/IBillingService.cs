using JustTaskTracker.Domain.Billing.DTOs;

namespace JustTaskTracker.Application.Billing.Abstractions;

/// <summary>
/// Billing operations for checkout, customer portal, and webhook parsing.
/// </summary>
public interface IBillingService
{
    /// <summary>
    /// Creates a Checkout Session in subscription mode for the given plan.
    /// </summary>
    /// <param name="userId">Application user id stored in Stripe metadata / client reference.</param>
    /// <param name="email">Customer email used when <paramref name="stripeCustomerId"/> is not provided.</param>
    /// <param name="planId">Catalog plan id that must map to a configured Stripe price.</param>
    /// <param name="stripeCustomerId">Existing Stripe customer id, when already known.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        Guid userId,
        string email,
        string planId,
        string? stripeCustomerId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Creates a Customer Portal session and returns its URL.
    /// </summary>
    Task<string> CreateCustomerPortalSessionAsync(
        string stripeCustomerId,
        CancellationToken ct = default);

    /// <summary>
    /// Loads a price and maps it to <see cref="PlanPriceDto"/>.
    /// </summary>
    Task<PlanPriceDto> GetPriceAsync(string stripePriceId, CancellationToken ct = default);

    /// <summary>
    /// Validates webhook signature and maps the payload to a domain event.
    /// </summary>
    Task<BillingWebhookEvent> ParseWebhookEventAsync(
        string payload,
        string stripeSignatureHeader,
        CancellationToken ct = default);
}
