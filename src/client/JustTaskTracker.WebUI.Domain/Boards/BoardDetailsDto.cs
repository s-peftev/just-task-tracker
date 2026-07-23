using JustTaskTracker.WebUI.Domain.Billing;
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
    BoardLimitsDto Limits,
    BoardExportOptions? ExportOptions = null,
    BoardExportStatus ReExportStatus = BoardExportStatus.None,
    BoardExportOptions? ReExportOptions = null,
    DateTime? ArchivedAtUtc = null);
