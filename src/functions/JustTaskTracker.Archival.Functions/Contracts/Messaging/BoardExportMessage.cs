using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Contracts.Messaging;

public record BoardExportMessage(Guid BoardId, ExportType Type, string CorrelationId);
