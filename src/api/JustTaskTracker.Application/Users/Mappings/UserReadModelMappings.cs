using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Application.Users.Mappings;

public static class UserReadModelMappings
{
    public static UserDto ToDto(this UserReadModel user, IProfilePhotoService profilePhotoService) =>
        new(
            user.Id,
            user.Email,
            user.DisplayName,
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id));

    public static UserDto? ToNullableDto(this UserReadModel? user, IProfilePhotoService profilePhotoService) =>
        user is null ? null : user.ToDto(profilePhotoService);

    public static UserWithRolesDto ToUserWithRolesDto(
        this UserReadModel user,
        IReadOnlyList<string> roles,
        IProfilePhotoService profilePhotoService) =>
        new(
            user.Id,
            user.Email,
            roles,
            user.DisplayName,
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildOriginalUrl(user.Id));
}
