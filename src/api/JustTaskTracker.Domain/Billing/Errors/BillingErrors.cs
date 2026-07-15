using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Billing.Errors;

public static class BillingErrors
{
    public static readonly Error SubscriptionAlreadyExists = new(
        nameof(SubscriptionAlreadyExists),
        ErrorType.Conflict,
        ["An active subscription already exists for this user."]);

    public static readonly Error SubscriptionNotFound = new(
        nameof(SubscriptionNotFound),
        ErrorType.NotFound,
        ["No active subscription was found for this user."]);

    public static readonly Error WebhookPayloadInvalid = new(
        nameof(WebhookPayloadInvalid),
        ErrorType.Validation,
        ["The Stripe webhook payload is missing required subscription fields."]);
}
