using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Users.ReadModels;

public record UserForBoardLookupReadModel(
    Guid Id,
    string Email,
    string? DisplayName,
    string? ProfilePhotoVersion,
    BoardMemberRole? BoardMemberRole);
