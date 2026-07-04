using JustTaskTracker.Archival.Functions.Contracts.DTOs;
using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Processing.Export;

public record BoardExportContext(
    Guid BoardId,
    BoardExportType Type,
    BoardExportOptions? Options,
    bool ShouldSkip = false,
    string? SkipReason = null);
