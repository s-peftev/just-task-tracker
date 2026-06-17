using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardLookupDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    BoardMemberRole UserRole,
    UserDto? Owner);
