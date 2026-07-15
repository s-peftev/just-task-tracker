namespace JustTaskTracker.WebUI.Domain.Billing;

public enum PlanCardSubscriptionNoticeKind
{
    Renews,
    Cancels,
    PastDue,
}

/// <summary>
/// Extra subscription copy shown under the status badge on the current paid plan card.
/// </summary>
public sealed record PlanCardSubscriptionNotice(
    PlanCardSubscriptionNoticeKind Kind,
    string Text);
