using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Application.Users.Mappings;

public static class UserReadModelMappings
{
    public static UserDto ToDto(this UserReadModel user, Func<UserReadModel, string?> profilePhotoUrlResolver) =>
        new(
            user.Id,
            user.Email,
            user.DisplayName,
            profilePhotoUrlResolver(user));

    public static UserDto? ToNullableDto(this UserReadModel? user, Func<UserReadModel, string?> profilePhotoUrlResolver) =>
        user is null ? null : user.ToDto(profilePhotoUrlResolver);

    public static UserWithRolesDto ToUserWithRolesDto(
        this UserReadModel user,
        IReadOnlyList<string> roles,
        Func<UserReadModel, string?> profilePhotoUrlResolver) =>
        new(
            user.Id,
            user.Email,
            roles,
            user.DisplayName,
            profilePhotoUrlResolver(user));
}
