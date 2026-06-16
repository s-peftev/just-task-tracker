namespace JustTaskTracker.Domain.Boards.DTOs.BoardTasks;

public record BoardTaskLookupDto(
    Guid Id,
    string Title,
    string? Description);
