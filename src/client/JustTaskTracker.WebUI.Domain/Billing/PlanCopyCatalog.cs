using JustTaskTracker.WebUI.Domain.Billing.Constants;

namespace JustTaskTracker.WebUI.Domain.Billing;

/// <summary>
/// Maps catalog <c>PlanId</c> values to UI marketing copy and accent styling.
/// Limit bullets are derived from <see cref="PlanLimitsDto"/>; feature bullets are plan-exclusive.
/// </summary>
public static class PlanCopyCatalog
{
    private sealed record PlanMeta(
        string Audience,
        string? AccentClass,
        IReadOnlyList<PlanHighlight> ExclusiveHighlights);

    private static readonly IReadOnlyList<PlanHighlight> ProExclusiveHighlights =
    [
        new(
            "Archive boards when you’re done",
            PlanHighlightIconKind.Archive),
        new(
            "Create downloadable copies of archived boards",
            PlanHighlightIconKind.DownloadCopy),
        new(
            "Choose and reconfigure what each archive copy includes",
            PlanHighlightIconKind.ConfigureCopy),
    ];

    private static readonly IReadOnlyDictionary<string, PlanMeta> ByPlanId =
        new Dictionary<string, PlanMeta>(StringComparer.OrdinalIgnoreCase)
        {
            [PlanIds.Free] = new(
                "Best for individuals tracking personal work.",
                PlanAccentClasses.Free,
                []),
            [PlanIds.Pro] = new(
                "Best for archive downloads and export control.",
                PlanAccentClasses.Pro,
                ProExclusiveHighlights),
        };

    public static PlanCopy GetOrEmpty(string planId, PlanLimitsDto limits)
    {
        var limitHighlights = BuildLimitHighlights(limits);

        if (string.IsNullOrWhiteSpace(planId) || !ByPlanId.TryGetValue(planId, out var meta))
            return new PlanCopy(string.Empty, limitHighlights);

        var highlights = limitHighlights
            .Concat(meta.ExclusiveHighlights)
            .ToList();

        return new PlanCopy(meta.Audience, highlights, meta.AccentClass);
    }

    private static IReadOnlyList<PlanHighlight> BuildLimitHighlights(PlanLimitsDto limits) =>
    [
        new(FormatBoards(limits.MaxBoards), PlanHighlightIconKind.Boards),
        new(FormatOrganize(limits.MaxColumnsPerBoard, limits.MaxTasksPerBoard), PlanHighlightIconKind.Organize),
        new(FormatMembers(limits.MaxMembersPerBoard), PlanHighlightIconKind.Collaborate),
    ];

    private static string FormatBoards(int? maxBoards) =>
        maxBoards is null
            ? "Create and manage unlimited boards for your work"
            : $"Create and manage up to {maxBoards} boards for your work";

    private static string FormatMembers(int? maxMembers) =>
        maxMembers is null
            ? "Collaborate with unlimited board members"
            : $"Collaborate with up to {maxMembers} board members on your boards";

    private static string FormatOrganize(int? maxColumns, int? maxTasks)
    {
        if (maxColumns is null && maxTasks is null)
            return "Organize work with unlimited columns and tasks.";

        var columns = maxColumns is null
            ? "unlimited columns"
            : $"up to {maxColumns} columns";
        var tasks = maxTasks is null
            ? "unlimited tasks"
            : $"up to {maxTasks} tasks";

        return $"Organize work with {columns}, and {tasks} on your boards.";
    }
}
