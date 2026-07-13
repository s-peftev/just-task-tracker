namespace JustTaskTracker.WebUI.Domain.Billing;

public record SubscriptionDetailsDto(
    string PlanId,
    string Status,
    bool CancelAtPeriodEnd,
    bool HasBillableSubscription,
    DateTime? CurrentPeriodStartUtc,
    DateTime? CurrentPeriodEndUtc);
