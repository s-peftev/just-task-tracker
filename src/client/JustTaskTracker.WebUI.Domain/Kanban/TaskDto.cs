using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Domain.Kanban;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    int Position,
    DateTime CreatedAtUtc,
    UserDto? Assignee,
    UserDto Reporter);
