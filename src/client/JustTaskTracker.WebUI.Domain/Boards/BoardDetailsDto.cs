using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    BoardMemberRole UserRole,
    IReadOnlyList<UserDto> Members,
    IReadOnlyList<ColumnDto> Columns);
