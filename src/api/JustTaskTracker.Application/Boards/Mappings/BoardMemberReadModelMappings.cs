using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Boards.DTOs.Boards;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardMemberReadModelMappings
{
    public static BoardMemberDto ToDto(
        this BoardMemberReadModel member,
        IProfilePhotoService profilePhotoService) =>
        new(
            member.User.ToDto(profilePhotoService),
            member.Role,
            member.JoinedAtUtc);
}
