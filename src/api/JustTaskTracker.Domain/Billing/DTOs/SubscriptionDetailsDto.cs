namespace JustTaskTracker.Domain.Billing.DTOs;

public record SubscriptionDetailsDto(
    string PlanId,
    string Status,
    bool CancelAtPeriodEnd,
    bool HasBillableSubscription,
    DateTime? CurrentPeriodStartUtc,
    DateTime? CurrentPeriodEndUtc);
