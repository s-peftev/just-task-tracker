using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Kanban.DTOs;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    IEnumerable<UserDto> Members,
    IEnumerable<ColumnDto> Columns);
