using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardMemberDto(
    UserDto User,
    BoardMemberRole Role,
    DateTime JoinedAtUtc);
