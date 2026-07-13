namespace JustTaskTracker.WebUI.Domain.Billing;

/// <summary>
/// A single marketing highlight on a plan card.
/// </summary>
/// <param name="Text">Bullet copy.</param>
/// <param name="Icon">Semantic icon for the bullet.</param>
/// <param name="IsInherited">
/// When <see langword="true"/>, this highlight comes from a narrower baseline plan
/// and should render muted on a wider plan.
/// </param>
public sealed record PlanHighlight(
    string Text,
    PlanHighlightIconKind Icon,
    bool IsInherited = false);
