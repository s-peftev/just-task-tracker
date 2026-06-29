using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardLookupDto(
    Guid Id,
    string Name,
    BoardMemberRole UserRole,
    string OwnerEmail,
    string? OwnerDisplayName);
