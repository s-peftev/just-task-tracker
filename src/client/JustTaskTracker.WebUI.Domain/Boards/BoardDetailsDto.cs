using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    bool IsArchived,
    BoardMemberRole UserRole,
    IReadOnlyList<ColumnDto> Columns,
    BoardExportStatus BoardExportStatus,
    BoardExportOptions? ExportOptions = null,
    DateTime? ArchivedAtUtc = null);
