using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardExportStatusInfo(
    Guid BoardId,
    BoardExportStatus Status,
    DateTime UpdatedAtUtc,
    string? ErrorMessage,
    BoardExportOptions? ExportOptions,
    BoardExportStatus ReExportStatus = BoardExportStatus.None,
    BoardExportOptions? ReExportOptions = null);
