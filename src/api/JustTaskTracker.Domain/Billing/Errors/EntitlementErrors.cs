using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Billing.Errors;

public static class EntitlementErrors
{
    public static readonly Error FeatureNotAvailable = new(
        nameof(FeatureNotAvailable),
        ErrorType.Forbidden,
        ["This feature is not available on your current plan."]);
}
