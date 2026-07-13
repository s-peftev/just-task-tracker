namespace JustTaskTracker.WebUI.Domain.Billing;

public record PlanDto(
    string PlanId,
    string PlanDisplayName,
    IReadOnlyList<string> Features);
