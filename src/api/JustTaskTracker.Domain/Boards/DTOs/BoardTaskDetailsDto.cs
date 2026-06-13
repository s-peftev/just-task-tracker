using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Boards.DTOs;

public record BoardTaskDetailsDto(
    Guid Id,
    Guid ColumnId,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    UserDto Reporter,
    string? Description = null,
    UserDto? Assignee = null);
