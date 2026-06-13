using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs;

public record BoardTaskDetailsDto(
    Guid Id,
    Guid ColumnId,
    string ColumnName,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    UserDto Reporter,
    BoardMemberRole UserRole,
    string? Description = null,
    UserDto? Assignee = null);
