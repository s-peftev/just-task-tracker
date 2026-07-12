namespace JustTaskTracker.WebUI.Domain.Billing;

public record SubscriptionDetailsDto(
    string PlanId,
    string Status,
    bool CancelAtPeriodEnd,
    bool CanManageInPortal,
    DateTime? CurrentPeriodStartUtc,
    DateTime? CurrentPeriodEndUtc);
