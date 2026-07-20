namespace JustTaskTracker.WebUI.Domain.Billing;

public record EntitlementDto(
    string PlanId,
    string PlanDisplayName,
    string Status,
    IReadOnlyList<string> Features);
