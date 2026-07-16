using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardMemberDto(
    UserDto User,
    bool IsGlobalAdmin,
    BoardMemberRole Role,
    DateTime JoinedAtUtc);
