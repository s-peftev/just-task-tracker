namespace JustTaskTracker.Domain.Boards.DTOs;

public record ColumnDto(
    Guid Id,
    string Name,
    int Position,
    IEnumerable<BoardTaskDto> BoardTasks);
