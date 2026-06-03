namespace JustTaskTracker.Domain.Kanban.DTOs;

public record ColumnDto(
    Guid Id,
    string Name,
    int Position,
    IEnumerable<TaskDto> Tasks);
