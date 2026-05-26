using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Common.Interfaces.Persistence.Repositories;
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
    IUserRoleRepository userRoleRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, Result<UserWithRolesDto>>
{
    public async Task<Result<UserWithRolesDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var existingUser = await userRepository.GetUserWithRolesByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (existingUser is null)
            return Result<UserWithRolesDto>.Success(await CreateAsync(ct));

        SyncProfile(existingUser);
        SyncRoles(existingUser);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<UserWithRolesDto>.Success(new UserWithRolesDto(
            existingUser.Id,
            existingUser.Email,
            existingUser.DisplayName,
            existingUser.UserRoles.Select(ur => ur.Role.Name).ToList()
        ));
    }

    private async Task<UserWithRolesDto> CreateAsync(CancellationToken ct)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            AzureAdObjectId = currentUser.AzureAdObjectId,
            Email = currentUser.Email,
            DisplayName = currentUser.DisplayName,
            CreatedAtUtc = DateTime.UtcNow
        };

        var rolesToAssign = ResolveRoles();

        userRepository.Add(user);

        foreach (var role in rolesToAssign)
            userRoleRepository.Add(new UserRole { UserId = user.Id, RoleId = role.Id });

        await unitOfWork.SaveChangesAsync(ct);

        return new UserWithRolesDto(
            user.Id,
            user.Email,
            user.DisplayName,
            rolesToAssign.Select(r => r.Name).ToList()
        );
    }

    private void SyncProfile(User user)
    {
        user.Email = currentUser.Email;
        user.DisplayName = currentUser.DisplayName;
    }

    private void SyncRoles(User user)
    {
        var actual = ResolveRoles().Select(r => r.Id).ToHashSet();
        var current = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();

        foreach (var roleId in actual.Except(current))
            userRoleRepository.Add(new UserRole { UserId = user.Id, RoleId = roleId });

        foreach (var userRole in user.UserRoles.Where(ur => !actual.Contains(ur.RoleId)).ToList())
            userRoleRepository.Remove(userRole);
    }

    private IReadOnlyList<RoleDefinition> ResolveRoles()
    {
        var tokenRoles = currentUser.AppRoles;

        if (tokenRoles.Count == 0)
            return [Roles.User];

        var matched = Roles.All
            .Where(r => tokenRoles.Contains(r.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        return matched.Count > 0 ? matched : [Roles.User];
    }
}