namespace JustTaskTracker.Domain.Billing.Constants;

public static class SubscriptionStatus
{
    public const string Incomplete = "incomplete";
    public const string IncompleteExpired = "incomplete_expired";
    public const string Trialing = "trialing";
    public const string Active = "active";
    public const string PastDue = "past_due";
    public const string Canceled = "canceled";
    public const string Unpaid = "unpaid";
    public const string Paused = "paused";

    private static readonly HashSet<string> BillableSet =
    [
        Active,
        Trialing,
        PastDue,
    ];

    private static readonly HashSet<string> KnownSet =
    [
        Incomplete,
        IncompleteExpired,
        Trialing,
        Active,
        PastDue,
        Canceled,
        Unpaid,
        Paused,
    ];

    public static bool IsBillable(string status) =>
        !string.IsNullOrEmpty(status) && BillableSet.Contains(status);

    /// <summary>Returns whether <paramref name="status"/> is one of Stripe's documented subscription statuses.</summary>
    public static bool IsKnown(string status) =>
        !string.IsNullOrEmpty(status) && KnownSet.Contains(status);

    public static IReadOnlyCollection<string> AllBillable => BillableSet;
}
