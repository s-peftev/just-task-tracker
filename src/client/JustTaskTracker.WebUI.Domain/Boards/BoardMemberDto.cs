using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardMemberDto(
    UserDto User,
    BoardMemberRole Role,
    DateTime JoinedAtUtc);
