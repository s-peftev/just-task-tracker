using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardLookupDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    BoardMemberRole UserRole,
    UserDto? Owner);
