using JustTaskTracker.Domain.Boards.DTOs.Columns;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    bool IsArchived,
    BoardMemberRole UserRole,
    IEnumerable<ColumnDto> Columns,
    BoardSerializationStatus BoardSerializationStatus,
    BoardArchiveExportOptions? ExportOptions = null,
    DateTime? ArchivedAtUtc = null);
