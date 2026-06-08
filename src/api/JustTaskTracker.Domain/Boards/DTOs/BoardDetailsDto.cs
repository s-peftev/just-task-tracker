using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    BoardMemberRole UserRole,
    IEnumerable<UserDto> Members,
    IEnumerable<ColumnDto> Columns);
