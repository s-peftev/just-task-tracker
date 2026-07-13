namespace JustTaskTracker.Domain.Billing.Enums;

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

    public static bool IsBillable(string status) =>
        !string.IsNullOrEmpty(status) && BillableSet.Contains(status);

    public static IReadOnlyCollection<string> AllBillable => BillableSet;
}
