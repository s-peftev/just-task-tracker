using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Kanban.Enums;

namespace JustTaskTracker.Domain.Kanban.DTOs;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    BoardMemberRole UserRole,
    IEnumerable<UserDto> Members,
    IEnumerable<ColumnDto> Columns);
