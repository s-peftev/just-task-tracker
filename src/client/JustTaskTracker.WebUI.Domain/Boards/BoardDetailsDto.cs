using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardDetailsDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    BoardMemberRole UserRole,
    IReadOnlyList<ColumnDto> Columns);
