using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Kanban.Enums;

namespace JustTaskTracker.WebUI.Domain.Kanban;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    BoardMemberRole UserRole,
    IReadOnlyList<UserDto> Members,
    IReadOnlyList<ColumnDto> Columns);
