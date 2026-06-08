using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record TaskDto(
    Guid Id,
    string Title,
    UserDto Reporter,
    int Position,
    DateTime CreatedAtUtc,
    string? Description,
    UserDto? Assignee);
