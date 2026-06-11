using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Boards.DTOs;

public record BoardTaskDetailsDto(
    Guid Id,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    UserDto Reporter,
    string? Description,
    UserDto? Assignee = null);
