using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardMemberReadModelMappings
{
    public static BoardMemberDto ToDto(
        this BoardMemberReadModel member,
        Func<UserReadModel, string?> profilePhotoUrlResolver) =>
        new(
            member.User.ToDto(profilePhotoUrlResolver),
            member.Role,
            member.JoinedAtUtc);
}
