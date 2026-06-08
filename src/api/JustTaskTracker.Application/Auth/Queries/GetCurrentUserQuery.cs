using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Auth.Queries;

public record GetCurrentUserQuery : IRequest<Result<UserWithRolesDto>>;

public class GetCurrentUserQueryHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository) 
    : IRequestHandler<GetCurrentUserQuery, Result<UserWithRolesDto>>
{
    public async Task<Result<UserWithRolesDto>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var userDto = await userRepository.GetUserDtoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (userDto is null)
            return Result<UserWithRolesDto>.Failure(GeneralErrors.NotFound);

        var rolesFromToken = currentUser.AppRoles ?? [];

        var userWithRoles = new UserWithRolesDto(
            userDto.Id,
            userDto.Email,
            rolesFromToken,
            userDto.DisplayName);

        return Result<UserWithRolesDto>.Success(userWithRoles);
    }
}