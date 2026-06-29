using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardSerializationStatusInfo(
    Guid BoardId,
    BoardSerializationStatus Status,
    DateTime UpdatedAtUtc,
    string? ErrorMessage,
    BoardArchiveExportOptions? ExportOptions);
