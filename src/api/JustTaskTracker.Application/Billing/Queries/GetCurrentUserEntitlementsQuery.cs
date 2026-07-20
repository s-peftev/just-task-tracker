using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Billing.Queries;

public record GetCurrentUserEntitlementsQuery : IRequest<Result<EntitlementDto>>;

public class GetCurrentUserEntitlementsQueryHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IEntitlementService entitlementService)
    : IRequestHandler<GetCurrentUserEntitlementsQuery, Result<EntitlementDto>>
{
    public async Task<Result<EntitlementDto>> Handle(GetCurrentUserEntitlementsQuery request, CancellationToken ct)
    {
        var userInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (userInfo is null)
            return Result<EntitlementDto>.Failure(GeneralErrors.NotFound);

        var rolesFromToken = currentUser.AppRoles ?? [];
        var entitlements = await entitlementService.GetEntitlementsAsync(userInfo.Id, rolesFromToken, ct);

        return Result<EntitlementDto>.Success(entitlements);
    }
}
