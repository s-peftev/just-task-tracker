namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardArchiveExportOptions(
    bool IncludeDescriptions,
    bool IncludeComments,
    bool IncludeAttachments,
    bool IncludeMembers);
