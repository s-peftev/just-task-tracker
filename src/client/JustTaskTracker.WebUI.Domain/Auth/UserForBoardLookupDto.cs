using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Auth;

public record UserForBoardLookupDto(
    Guid Id,
    string Email,
    string? DisplayName,
    string? ProfilePhotoUrl,
    BoardMemberRole? BoardMemberRole);
