using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Application.Users.Mappings;

public static class UserForBoardLookupReadModelMappings
{
    public static UserForBoardLookupDto ToDto(
        this UserForBoardLookupReadModel user,
        IProfilePhotoService profilePhotoService) =>
        new(
            user.Id,
            user.Email,
            user.DisplayName,
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id),
            user.BoardMemberRole);
}
