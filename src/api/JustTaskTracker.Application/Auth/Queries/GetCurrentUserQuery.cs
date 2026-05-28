using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence.Repositories;
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
        var user = await userRepository.GetUserWithRolesDtoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (user is null)
            return Result<UserWithRolesDto>.Failure(GeneralErrors.NotFound);

        return Result<UserWithRolesDto>.Success(user);
    }
}