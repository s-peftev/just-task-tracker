using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardMemberDto(
    UserDto User,
    bool IsGlobalAdmin,
    BoardMemberRole Role,
    DateTime JoinedAtUtc);
