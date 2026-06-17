using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;

namespace JustTaskTracker.Domain.Boards.DTOs.Columns;

public record ColumnDto(
    Guid Id,
    string Name,
    int Position,
    IEnumerable<BoardTaskPreviewDto> BoardTasks);
