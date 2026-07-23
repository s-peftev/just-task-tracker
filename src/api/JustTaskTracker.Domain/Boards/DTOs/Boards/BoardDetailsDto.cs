using JustTaskTracker.Domain.Billing.DTOs;
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
    BoardExportStatus BoardExportStatus,
    BoardLimitsDto Limits,
    BoardExportOptions? ExportOptions = null,
    BoardExportStatus ReExportStatus = BoardExportStatus.None,
    BoardExportOptions? ReExportOptions = null,
    DateTime? ArchivedAtUtc = null);
