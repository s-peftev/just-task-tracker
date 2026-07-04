namespace JustTaskTracker.WebUI.Domain.Boards;

public static class BoardExportOptionsFormatting
{
    private static readonly (string Label, Func<BoardExportOptions, bool> IsIncluded)[] OptionEntries =
    [
        ("Task descriptions", options => options.IncludeDescriptions),
        ("Comments", options => options.IncludeComments),
        ("Task attachments", options => options.IncludeAttachments),
        ("Board members", options => options.IncludeMembers),
    ];

    public static IEnumerable<(string Label, bool IsIncluded)> GetOptionStates(BoardExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return OptionEntries.Select(entry => (entry.Label, entry.IsIncluded(options)));
    }

    public static bool AreEqual(BoardExportOptions left, BoardExportOptions right) =>
        left.IncludeDescriptions == right.IncludeDescriptions
        && left.IncludeComments == right.IncludeComments
        && left.IncludeAttachments == right.IncludeAttachments
        && left.IncludeMembers == right.IncludeMembers;
}
