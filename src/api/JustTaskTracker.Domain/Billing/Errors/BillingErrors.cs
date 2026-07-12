using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Billing.Errors;

public static class BillingErrors
{
    public static readonly Error SubscriptionAlreadyExists = new(
        nameof(SubscriptionAlreadyExists),
        ErrorType.Conflict,
        ["An active subscription already exists for this user."]);
}
