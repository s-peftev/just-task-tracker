using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardLookupDto(
    Guid Id,
    string Name,
    bool IsArchived,
    BoardMemberRole UserRole,
    string OwnerEmail,
    BoardExportStatus BoardExportStatus,
    string? OwnerDisplayName,
    DateTime? ArchivedAtUtc);
