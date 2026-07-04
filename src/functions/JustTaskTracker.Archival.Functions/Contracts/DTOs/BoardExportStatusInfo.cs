using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Contracts.DTOs;

public record BoardExportStatusInfo(
    Guid BoardId,
    DateTime UpdatedAtUtc,
    BoardExportStatus ExportStatus,
    BoardExportOptions? ExportOptions,
    BoardExportStatus ReExportStatus = BoardExportStatus.None,
    BoardExportOptions? ReExportOptions = null,
    string? ErrorMessage = null);
