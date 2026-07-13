namespace JustTaskTracker.Domain.Billing.DTOs;

public record PlanPriceDto(
    string Currency,
    long UnitAmount,
    string Interval);
