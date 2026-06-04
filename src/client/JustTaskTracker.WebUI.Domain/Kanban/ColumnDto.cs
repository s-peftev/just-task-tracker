namespace JustTaskTracker.WebUI.Domain.Kanban;

public record ColumnDto(
    Guid Id,
    string Name,
    int Position,
    IReadOnlyList<TaskDto> Tasks);
