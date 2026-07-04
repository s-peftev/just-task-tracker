using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardExportStatusInfo(
    Guid BoardId,
    DateTime UpdatedAtUtc,
    BoardExportStatus ExportStatus,
    BoardExportOptions? ExportOptions,
    BoardExportStatus ReExportStatus = BoardExportStatus.None,
    BoardExportOptions? ReExportOptions = null,
    string? ErrorMessage = null);
