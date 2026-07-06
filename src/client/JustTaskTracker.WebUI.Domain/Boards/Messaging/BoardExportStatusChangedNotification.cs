using System.Text.Json.Serialization;
using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards.Messaging;

public record BoardExportStatusChangedNotification(
    [property: JsonPropertyName("boardId")] Guid BoardId,
    [property: JsonPropertyName("status")] BoardExportStatus Status,
    [property: JsonPropertyName("exportOptions")] BoardExportOptions? ExportOptions = null);
