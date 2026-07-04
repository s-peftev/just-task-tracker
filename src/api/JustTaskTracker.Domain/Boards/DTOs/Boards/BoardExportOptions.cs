namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardExportOptions(
    bool IncludeDescriptions,
    bool IncludeComments,
    bool IncludeAttachments,
    bool IncludeMembers);
