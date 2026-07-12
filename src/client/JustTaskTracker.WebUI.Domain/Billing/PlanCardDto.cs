namespace JustTaskTracker.WebUI.Domain.Billing;

public record PlanCardDto(
    string PlanId,
    string PlanDisplayName,
    IReadOnlyList<string> Features,
    PlanPriceDto? Price);
