using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public static class BoardExportStatusFormatting
{
    /// <summary>
    /// Tooltip text for archived boards only — preparation of a downloadable copy (not archiving itself).
    /// </summary>
    public static string GetDescription(BoardExportStatus status) =>
        status switch
        {
            BoardExportStatus.Requested =>
                "Export requested — waiting to be scheduled.",
            BoardExportStatus.Pending =>
                "Queued to create a downloadable copy of this board.",
            BoardExportStatus.Processing =>
                "Creating a downloadable copy of this board. You'll be able to save it to your device when it's ready.",
            BoardExportStatus.Completed =>
                "Download ready — you can save a copy of this board to your device.",
            BoardExportStatus.Failed =>
                "Couldn't create a downloadable copy. We'll try again automatically.",
            _ => status.ToString(),
        };

    public static string GetAriaLabel(BoardExportStatus status, BoardExportOptions? exportOptions)
    {
        var description = GetDescription(status);

        if (exportOptions is null)
            return description;

        var optionsSummary = string.Join(
            ", ",
            BoardExportOptionsFormatting.GetOptionStates(exportOptions)
                .Select(entry => $"{entry.Label}: {(entry.IsIncluded ? "yes" : "no")}"));

        return $"{description} Options: {optionsSummary}.";
    }
}
