using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Kanban.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    int Position,
    DateTime CreatedAtUtc,
    UserDto? Assignee,
    UserDto Reporter);
