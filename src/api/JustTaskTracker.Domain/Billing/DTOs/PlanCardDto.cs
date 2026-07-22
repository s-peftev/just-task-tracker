namespace JustTaskTracker.Domain.Billing.DTOs;

public record PlanCardDto(
    string PlanId,
    string PlanDisplayName,
    IReadOnlyList<string> Features,
    PlanLimitsDto Limits,
    PlanPriceDto? Price);
