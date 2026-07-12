namespace JustTaskTracker.Domain.Billing.DTOs;

public record SubscriptionDetailsDto(
    string PlanId,
    string PlanDisplayName,
    string Status,
    bool CancelAtPeriodEnd,
    bool CanManageInPortal,
    DateTime? CurrentPeriodStartUtc,
    DateTime? CurrentPeriodEndUtc);
