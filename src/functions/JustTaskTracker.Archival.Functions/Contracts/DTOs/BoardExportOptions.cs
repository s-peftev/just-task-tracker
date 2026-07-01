namespace JustTaskTracker.Archival.Functions.Contracts.DTOs;

public record BoardExportOptions(
    bool IncludeDescriptions,
    bool IncludeComments,
    bool IncludeAttachments,
    bool IncludeMembers);
