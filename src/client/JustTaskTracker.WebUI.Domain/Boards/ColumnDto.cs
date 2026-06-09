namespace JustTaskTracker.WebUI.Domain.Boards;

public record ColumnDto(
    Guid Id,
    string Name,
    int Position,
    IReadOnlyList<TaskDto> BoardTasks);
