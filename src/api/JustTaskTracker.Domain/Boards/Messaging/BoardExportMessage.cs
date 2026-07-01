using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.Messaging;

public record BoardExportMessage(Guid BoardId, BoardExportType Type, string CorrelationId);
