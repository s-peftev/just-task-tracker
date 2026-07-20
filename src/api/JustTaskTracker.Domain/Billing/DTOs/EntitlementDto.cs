namespace JustTaskTracker.Domain.Billing.DTOs;

public record EntitlementDto(
    string PlanId,
    string PlanDisplayName,
    string Status,
    IReadOnlyList<string> Features);
