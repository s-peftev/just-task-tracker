using JustTaskTracker.WebUI.Domain.Billing.Constants;

namespace JustTaskTracker.WebUI.Domain.Billing;

/// <summary>
/// Maps catalog <c>PlanId</c> values to UI marketing copy and accent styling.
/// Wider plans compose a narrower baseline (muted inherited highlights) plus exclusive ones.
/// </summary>
public static class PlanCopyCatalog
{
    private static readonly IReadOnlyList<PlanHighlight> FreeHighlights =
    [
        new(
            "Create and manage unlimited boards for your work",
            PlanHighlightIconKind.Boards),
        new(
            "Organize work with columns, tasks, assignees, and drag-and-drop",
            PlanHighlightIconKind.Organize),
        new(
            "Collaborate with unlimited board members",
            PlanHighlightIconKind.Collaborate),
        new(
            "Discuss work in task comments and share file attachments",
            PlanHighlightIconKind.Discuss),
        new(
            "Archive boards when you’re done",
            PlanHighlightIconKind.Archive),
    ];

    private static readonly IReadOnlyList<PlanHighlight> ProExclusiveHighlights =
    [
        new(
            "Create downloadable copies of archived boards",
            PlanHighlightIconKind.DownloadCopy),
        new(
            "Choose and reconfigure what each archive copy includes",
            PlanHighlightIconKind.ConfigureCopy),
    ];

    private static readonly PlanCopy Free = new(
        "Best for individuals tracking personal work.",
        FreeHighlights,
        PlanAccentClasses.Free);

    private static readonly PlanCopy Pro = new(
        "Best for owners who need archive downloads and export control.",
        ExtendWithBaseline(FreeHighlights, ProExclusiveHighlights),
        PlanAccentClasses.Pro);

    private static readonly IReadOnlyDictionary<string, PlanCopy> ByPlanId =
        new Dictionary<string, PlanCopy>(StringComparer.OrdinalIgnoreCase)
        {
            [PlanIds.Free] = Free,
            [PlanIds.Pro] = Pro,
        };

    public static PlanCopy? TryGet(string planId)
    {
        if (string.IsNullOrWhiteSpace(planId))
            return null;

        return ByPlanId.TryGetValue(planId, out var copy) ? copy : null;
    }

    public static PlanCopy GetOrEmpty(string planId) =>
        TryGet(planId) ?? new PlanCopy(string.Empty, []);

    /// <summary>
    /// Builds a wider plan's highlight list: baseline items marked inherited (muted),
    /// then plan-exclusive items at full emphasis. Adding a mid-tier later is the same pattern.
    /// </summary>
    private static IReadOnlyList<PlanHighlight> ExtendWithBaseline(
        IReadOnlyList<PlanHighlight> baseline,
        IReadOnlyList<PlanHighlight> exclusive) =>
        baseline
            .Select(highlight => highlight with { IsInherited = true })
            .Concat(exclusive)
            .ToList();
}
