using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Domain.Billing.Constants;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Billing.Errors;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Application.Billing.Webhooks.Handlers;

public sealed class CustomerSubscriptionUpdatedWebhookHandler(
    ISubscriptionRepository subscriptionRepository)
    : IBillingWebhookEventHandler
{
    public string EventType => StripeWebhookEventTypes.CustomerSubscriptionUpdated;

    public async Task<Result> HandleAsync(BillingWebhookEvent billingEvent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingEvent.StripeSubscriptionId) || string.IsNullOrWhiteSpace(billingEvent.Status))
            return Result.Failure(BillingErrors.WebhookPayloadInvalid);

        var subscription = await subscriptionRepository.GetByStripeSubscriptionIdAsync(
            billingEvent.StripeSubscriptionId,
            ct);

        if (subscription is null)
            return Result.Success();

        if (!subscription.Status.Equals(billingEvent.Status, StringComparison.Ordinal))
        {
            if (!SubscriptionStatus.IsKnown(billingEvent.Status))
                return Result.Failure(BillingErrors.UnknownSubscriptionStatus);

            subscription.Status = billingEvent.Status;
        }

        if (subscription.CancelAtPeriodEnd != billingEvent.CancelAtPeriodEnd)
            subscription.CancelAtPeriodEnd = billingEvent.CancelAtPeriodEnd;

        // Absent when Stripe reports no subscription items (e.g. malformed event); keep the last known period in that case.
        if (billingEvent.CurrentPeriodStartUtc is { } periodStart && subscription.CurrentPeriodStartUtc != periodStart)
            subscription.CurrentPeriodStartUtc = periodStart;

        if (billingEvent.CurrentPeriodEndUtc is { } periodEnd && subscription.CurrentPeriodEndUtc != periodEnd)
            subscription.CurrentPeriodEndUtc = periodEnd;

        return Result.Success();
    }
}
