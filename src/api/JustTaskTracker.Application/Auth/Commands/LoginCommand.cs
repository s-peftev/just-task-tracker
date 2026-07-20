using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Auth.Constants;
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

        var userInfo = new UserWithRolesDto(
            user.Id,
            user.Email,
            rolesFromToken,
            user.DisplayName,
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildOriginalUrl(user.Id, user.ProfilePhotoVersion));

        return Result<UserWithRolesDto>.Success(userInfo);
    }

    private User CreateUser()
    {
        var user = new User
        {
            AzureAdObjectId = currentUser.AzureAdObjectId,
            Email = currentUser.Email,
            DisplayName = currentUser.DisplayName,
        };

        SyncGlobalRoles(user);
        userRepository.Add(user);

        return user;
    }

    private void SyncProfile(User user)
    {
        user.Email = currentUser.Email;
        user.DisplayName = currentUser.DisplayName;
        SyncGlobalRoles(user);
    }

    /// <summary>
    /// Full replace: DB roles become exactly the known roles from the token
    /// (empty token clears all persisted roles).
    /// </summary>
    private void SyncGlobalRoles(User user)
    {
        var desiredRoles = ResolveKnownRoles(currentUser.AppRoles);

        foreach (var existing in user.GlobalRoles.ToList())
        {
            if (!desiredRoles.Contains(existing.Role, StringComparer.Ordinal))
                user.GlobalRoles.Remove(existing);
        }

        foreach (var role in desiredRoles)
        {
            if (user.GlobalRoles.Any(r => r.Role.Equals(role, StringComparison.Ordinal)))
                continue;

            user.GlobalRoles.Add(new UserGlobalRole
            {
                UserId = user.Id,
                Role = role,
            });
        }
    }

    /// <summary>
    /// Maps token role claims onto <see cref="Roles.All"/> (CHECK-constrained values).
    /// Unknown claims are ignored so SaveChanges does not fail the login.
    /// </summary>
    private static IReadOnlyList<string> ResolveKnownRoles(IReadOnlyList<string>? appRoles)
    {
        if (appRoles is null || appRoles.Count == 0)
            return [];

        var known = new HashSet<string>(StringComparer.Ordinal);

        foreach (var claim in appRoles)
        {
            var match = Roles.All.FirstOrDefault(r =>
                r.Equals(claim, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
                known.Add(match);
        }

        return known.ToList();
    }
}
