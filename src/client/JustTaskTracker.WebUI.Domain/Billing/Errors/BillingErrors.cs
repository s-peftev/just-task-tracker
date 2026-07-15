using JustTaskTracker.WebUI.Domain.Common;

namespace JustTaskTracker.WebUI.Domain.Billing.Errors;

/// <summary>
/// Client mirror of API billing error codes / messages for matching <c>ApiError.Code</c>.
/// </summary>
public static class BillingErrors
{
    public const string SubscriptionAlreadyExists = nameof(SubscriptionAlreadyExists);

    public static readonly ApiErrorType SubscriptionAlreadyExistsType = ApiErrorType.Conflict;

    public static readonly IReadOnlyList<string> SubscriptionAlreadyExistsDetails =
        ["An active subscription already exists for this user."];

    public const string SubscriptionNotFound = nameof(SubscriptionNotFound);

    public static readonly ApiErrorType SubscriptionNotFoundType = ApiErrorType.NotFound;

    public static readonly IReadOnlyList<string> SubscriptionNotFoundDetails =
        ["No active subscription was found for this user."];
}
