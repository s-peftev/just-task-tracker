using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Kanban.Enums;

namespace JustTaskTracker.WebUI.Domain.Kanban;

public record BoardLookupDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    UserDto? Owner,
    BoardMemberRole UserRole);
