using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.Messaging;

public record BoardExportStatusChangedNotification(
    Guid BoardId,
    BoardExportStatus BoardExportStatus,
    BoardExportStatus ReExportStatus);
