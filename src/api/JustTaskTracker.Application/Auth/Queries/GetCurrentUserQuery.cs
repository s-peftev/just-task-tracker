using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Auth.Queries;

public record GetCurrentUserQuery : IRequest<Result<UserWithRolesDto>>;

public class GetCurrentUserQueryHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IProfilePhotoService profilePhotoService) 
    : IRequestHandler<GetCurrentUserQuery, Result<UserWithRolesDto>>
{
    public async Task<Result<UserWithRolesDto>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var userInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (userInfo is null)
            return Result<UserWithRolesDto>.Failure(GeneralErrors.NotFound);

        var rolesFromToken = currentUser.AppRoles ?? [];

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildOriginalUrl(user.Id, user.ProfilePhotoVersion);

        return Result<UserWithRolesDto>.Success(userInfo.ToUserWithRolesDto(rolesFromToken, profilePhotoUrlResolver));
    }
}