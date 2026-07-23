namespace JustTaskTracker.WebUI.Domain.Billing;

/// <summary>
/// A single marketing highlight on a plan card.
/// </summary>
/// <param name="Text">Bullet copy.</param>
/// <param name="Icon">Semantic icon for the bullet.</param>
public sealed record PlanHighlight(
    string Text,
    PlanHighlightIconKind Icon);
