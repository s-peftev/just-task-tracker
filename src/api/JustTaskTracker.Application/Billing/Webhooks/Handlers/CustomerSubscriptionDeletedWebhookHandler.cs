using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Domain.Billing.Constants;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Billing.Errors;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Application.Billing.Webhooks.Handlers;

public sealed class CustomerSubscriptionDeletedWebhookHandler(
    ISubscriptionRepository subscriptionRepository)
    : IBillingWebhookEventHandler
{
    public string EventType => StripeWebhookEventTypes.CustomerSubscriptionDeleted;

    public async Task<Result> HandleAsync(BillingWebhookEvent billingEvent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(billingEvent.StripeSubscriptionId))
            return Result.Failure(BillingErrors.WebhookPayloadInvalid);

        var subscription = await subscriptionRepository.GetByStripeSubscriptionIdAsync(
            billingEvent.StripeSubscriptionId,
            ct);

        if (subscription is null)
            return Result.Success();

        subscription.Status = SubscriptionStatus.Canceled;

        return Result.Success();
    }
}
