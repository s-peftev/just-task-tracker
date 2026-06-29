using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public static class BoardSerializationStatusFormatting
{
    /// <summary>
    /// Tooltip text for archived boards only — preparation of a downloadable copy (not archiving itself).
    /// </summary>
    public static string GetDescription(BoardSerializationStatus status) =>
        status switch
        {
            BoardSerializationStatus.Pending =>
                "Queued to create a downloadable copy of this board.",
            BoardSerializationStatus.Processing =>
                "Creating a downloadable copy of this board. You'll be able to save it to your device when it's ready.",
            BoardSerializationStatus.Completed =>
                "Download ready — you can save a copy of this board to your device.",
            BoardSerializationStatus.Failed =>
                "Couldn't create a downloadable copy. We'll try again automatically.",
            _ => status.ToString(),
        };
}
