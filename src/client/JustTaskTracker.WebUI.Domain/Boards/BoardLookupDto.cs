using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardLookupDto(
    Guid Id,
    string Name,
    bool IsArchived,
    BoardMemberRole UserRole,
    string OwnerEmail,
    BoardExportStatus BoardExportStatus,
    string? OwnerDisplayName,
    DateTime? ArchivedAtUtc);
