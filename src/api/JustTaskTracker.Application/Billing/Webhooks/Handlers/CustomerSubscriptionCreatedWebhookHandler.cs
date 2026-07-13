using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Domain.Billing.Constants;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Billing.Entities;
using JustTaskTracker.Domain.Billing.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Billing.Webhooks.Handlers;

public sealed class CustomerSubscriptionCreatedWebhookHandler(
    ISubscriptionRepository subscriptionRepository,
    IUserRepository userRepository,
    ILogger<CustomerSubscriptionCreatedWebhookHandler> logger)
    : IBillingWebhookEventHandler
{
    public string EventType => StripeWebhookEventTypes.CustomerSubscriptionCreated;

    public async Task<Result> HandleAsync(BillingWebhookEvent billingEvent, CancellationToken ct = default)
    {
        if (!TryGetRequiredFields(billingEvent, out var userId, out var planId, out var customerId, out var subscriptionId, out var status))
            return Result.Failure(BillingErrors.WebhookPayloadInvalid);

        // Incomplete / non-billable statuses are acknowledged but not persisted yet.
        // Lifecycle sync (e.g. incomplete → active) belongs in a future subscription.updated handler.
        if (!SubscriptionStatus.IsBillable(status))
        {
            logger.LogInformation(
                "Ignoring non-billable subscription status '{Status}' for Stripe subscription {StripeSubscriptionId}.",
                status,
                subscriptionId);

            return Result.Success();
        }

        if (await subscriptionRepository.ExistsByStripeSubscriptionIdAsync(subscriptionId, ct))
            return Result.Success();

        if (await userRepository.GetByIdAsync(userId, ct) is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (await subscriptionRepository.GetSubscriptionByUserIdAsync(userId, ct) is not null)
            return Result.Failure(BillingErrors.SubscriptionAlreadyExists);

        subscriptionRepository.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = planId,
            StripeCustomerId = customerId,
            StripeSubscriptionId = subscriptionId,
            Status = status,
            CurrentPeriodStartUtc = billingEvent.CurrentPeriodStartUtc,
            CurrentPeriodEndUtc = billingEvent.CurrentPeriodEndUtc,
            CancelAtPeriodEnd = billingEvent.CancelAtPeriodEnd,
        });

        return Result.Success();
    }

    private static bool TryGetRequiredFields(
        BillingWebhookEvent billingEvent,
        out Guid userId,
        out string planId,
        out string customerId,
        out string subscriptionId,
        out string status)
    {
        userId = billingEvent.UserId ?? Guid.Empty;
        planId = billingEvent.PlanId ?? string.Empty;
        customerId = billingEvent.StripeCustomerId ?? string.Empty;
        subscriptionId = billingEvent.StripeSubscriptionId ?? string.Empty;
        status = billingEvent.Status ?? string.Empty;

        return billingEvent.UserId is not null
            && !string.IsNullOrWhiteSpace(planId)
            && !string.IsNullOrWhiteSpace(customerId)
            && !string.IsNullOrWhiteSpace(subscriptionId)
            && !string.IsNullOrWhiteSpace(status);
    }
}
