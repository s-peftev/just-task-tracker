using JustTaskTracker.WebUI.Domain.Common;

namespace JustTaskTracker.WebUI.Domain.Billing.Errors;

/// <summary>
/// Client mirror of API entitlement error codes / messages for matching <c>ApiError.Code</c>.
/// </summary>
public static class EntitlementErrors
{
    public const string FeatureNotAvailable = nameof(FeatureNotAvailable);

    public static readonly ApiErrorType FeatureNotAvailableType = ApiErrorType.Forbidden;

    public static readonly IReadOnlyList<string> FeatureNotAvailableDetails =
        ["This feature is not available on your current plan."];

    public const string LimitReached = nameof(LimitReached);

    public static readonly ApiErrorType LimitReachedType = ApiErrorType.Forbidden;

    public static readonly IReadOnlyList<string> LimitReachedDetails =
        ["You have reached the limit for your current plan."];

    public const string BoardLimitReached = nameof(BoardLimitReached);

    public static readonly ApiErrorType BoardLimitReachedType = ApiErrorType.Forbidden;

    public static readonly IReadOnlyList<string> BoardLimitReachedDetails =
        ["This board has reached the limit for the owner's plan."];
}
