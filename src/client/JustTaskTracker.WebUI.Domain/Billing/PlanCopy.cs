namespace JustTaskTracker.WebUI.Domain.Billing;

/// <summary>
/// Client-owned marketing copy for a plan card. Looked up by <c>PlanId</c> from the API;
/// never treat this as the source of truth for entitlements or pricing.
/// </summary>
/// <param name="Audience">Short line describing who the plan is for.</param>
/// <param name="Highlights">Marketing bullets shown on the card.</param>
/// <param name="AccentClass">Optional CSS modifier for plan accent color.</param>
public sealed record PlanCopy(
    string Audience,
    IReadOnlyList<PlanHighlight> Highlights,
    string? AccentClass = null);
