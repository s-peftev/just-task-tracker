using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs;

public record BoardLookupDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    UserDto? Owner,
    BoardMemberRole UserRole);
