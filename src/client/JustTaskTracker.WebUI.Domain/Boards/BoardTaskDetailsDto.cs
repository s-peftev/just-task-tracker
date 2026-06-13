using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardTaskDetailsDto(
    Guid Id,
    Guid ColumnId,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    UserDto Reporter,
    string? Description = null,
    UserDto? Assignee = null);
