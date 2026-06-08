using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Boards.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    UserDto Reporter,
    string? Description,
    UserDto? Assignee);
