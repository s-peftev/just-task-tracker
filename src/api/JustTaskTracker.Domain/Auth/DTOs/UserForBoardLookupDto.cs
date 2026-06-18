using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Auth.DTOs;

public record UserForBoardLookupDto(
    Guid Id,
    string Email,
    string? DisplayName,
    BoardMemberRole? BoardMemberRole);
