namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardExportOptions(
    bool IncludeDescriptions,
    bool IncludeComments,
    bool IncludeAttachments,
    bool IncludeMembers);
