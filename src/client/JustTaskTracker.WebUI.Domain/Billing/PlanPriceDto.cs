namespace JustTaskTracker.WebUI.Domain.Billing;

public record PlanPriceDto(
    string Currency,
    long UnitAmount,
    string Interval);
