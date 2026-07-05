using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.Messaging;

public record BoardExportStatusChangedNotification(
    Guid BoardId,
    BoardExportStatus Status,
    BoardExportOptions? ExportOptions = null);
