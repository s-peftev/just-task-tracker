using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Application.Billing.Webhooks;

/// <summary>
/// Handles a single Stripe event type. Register additional implementations in DI to extend coverage.
/// </summary>
public interface IBillingWebhookEventHandler
{
    string EventType { get; }

    Task<Result> HandleAsync(BillingWebhookEvent billingEvent, CancellationToken ct = default);
}
