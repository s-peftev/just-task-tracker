namespace JustTaskTracker.Domain.Boards.DTOs.Archiving;

public record BoardExportColumnDto(
    Guid Id,
    string Name,
    int Position,
    IReadOnlyList<BoardExportTaskDto> Tasks);
