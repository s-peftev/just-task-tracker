using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardMemberReadModelMappings
{
    public static BoardMemberDto ToDto(
        this BoardMemberReadModel member,
        Func<UserReadModel, string?> profilePhotoUrlResolver,
        Func<IReadOnlyList<string>, bool> isGlobalAdminResolver) =>
        new(
            member.User.ToDto(profilePhotoUrlResolver),
            isGlobalAdminResolver(member.GlobalRoles),
            member.Role,
            member.JoinedAtUtc);
}
