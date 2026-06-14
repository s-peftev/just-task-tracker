using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardTaskDetailsDto(
    Guid Id,
    Guid ColumnId,
    string ColumnName,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    UserDto Reporter,
    BoardMemberRole UserRole,
    IReadOnlyList<BoardTaskAttachmentDto> Attachments,
    string? Description = null,
    UserDto? Assignee = null);
