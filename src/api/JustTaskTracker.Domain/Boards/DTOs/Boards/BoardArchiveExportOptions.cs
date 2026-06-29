namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardArchiveExportOptions(
    bool IncludeDescriptions,
    bool IncludeComments,
    bool IncludeAttachments,
    bool IncludeMembers);
