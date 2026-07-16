using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Application.Users.Mappings;

public static class UserForBoardLookupReadModelMappings
{
    public static UserForBoardLookupDto ToDto(
        this UserForBoardLookupReadModel user,
        Func<UserForBoardLookupReadModel, string?> profilePhotoUrlResolver,
        Func<IReadOnlyList<string>, bool> isGlobalAdminResolver) =>
        new(
            user.Id,
            user.Email,
            user.DisplayName,
            profilePhotoUrlResolver(user),
            isGlobalAdminResolver(user.GlobalRoles),
            user.BoardMemberRole);
}
