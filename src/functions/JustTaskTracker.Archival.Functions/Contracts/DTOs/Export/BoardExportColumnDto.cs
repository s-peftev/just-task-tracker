namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public record BoardExportColumnDto(
    Guid Id,
    string Name,
    int Position,
    IReadOnlyList<BoardExportTaskDto> Tasks);
