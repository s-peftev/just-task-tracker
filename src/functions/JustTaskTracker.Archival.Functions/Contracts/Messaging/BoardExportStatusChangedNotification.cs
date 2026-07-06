using JustTaskTracker.Archival.Functions.Contracts.DTOs;
using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Contracts.Messaging;

public record BoardExportStatusChangedNotification(
    Guid BoardId,
    BoardExportStatus Status,
    BoardExportOptions? ExportOptions = null);
