using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Common.Results;
using MediatR;

namespace JustTaskTracker.Application.Auth.Commands;

public record LoginCommand : IRequest<Result<UserWithRolesDto>>;

public class LoginCommandHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<LoginCommand, Result<UserWithRolesDto>>
{
    public async Task<Result<UserWithRolesDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetUserByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        user ??= CreateUser();

        SyncProfile(user);
        await unitOfWork.SaveChangesAsync(ct);

        var rolesFromToken = currentUser.AppRoles ?? [];

        var userInfo = new UserReadModel(user.Id, user.Email, user.DisplayName, user.ProfilePhotoVersion);

        return Result<UserWithRolesDto>.Success(userInfo.ToUserWithRolesDto(rolesFromToken, profilePhotoService));
    }

    private User CreateUser()
    {
        var user = new User
        {
            AzureAdObjectId = currentUser.AzureAdObjectId,
            Email = currentUser.Email,
            DisplayName = currentUser.DisplayName,
        };

        userRepository.Add(user);

        return user;
    }

    private void SyncProfile(User user)
    {
        user.Email = currentUser.Email;
        user.DisplayName = currentUser.DisplayName;
    }
}