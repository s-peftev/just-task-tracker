namespace JustTaskTracker.Application.Billing.ReadModels;

public record SubscriptionDetailsReadModel(
    string PlanId,
    string Status,
    bool CancelAtPeriodEnd,
    DateTime? CurrentPeriodStartUtc,
    DateTime? CurrentPeriodEndUtc);
