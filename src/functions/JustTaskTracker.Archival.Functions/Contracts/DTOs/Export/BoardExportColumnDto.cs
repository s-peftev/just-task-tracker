namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public sealed record BoardExportColumnDto(
    Guid Id,
    string Name,
    int Position,
    IReadOnlyList<BoardExportTaskDto> Tasks);
