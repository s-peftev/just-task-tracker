namespace JustTaskTracker.Domain.Boards.DTOs;

public record BoardTaskLookupDto(
    Guid Id,
    string Title,
    int Position);
