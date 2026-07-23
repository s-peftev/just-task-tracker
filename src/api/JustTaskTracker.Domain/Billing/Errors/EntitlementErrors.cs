using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Billing.Errors;

public static class EntitlementErrors
{
    public static readonly Error FeatureNotAvailable = new(
        nameof(FeatureNotAvailable),
        ErrorType.Forbidden,
        ["This feature is not available on your current plan."]);

    public static readonly Error LimitReached = new(
        nameof(LimitReached),
        ErrorType.Forbidden,
        ["You have reached the limit for your current plan."]);

    public static readonly Error BoardLimitReached = new(
        nameof(BoardLimitReached),
        ErrorType.Forbidden,
        ["This board has reached the limit for the owner's plan."]);
}
