using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Billing.Queries;

public record GetCurrentUserEntitlementsQuery : IRequest<Result<PlanDto>>;

public class GetCurrentUserEntitlementsQueryHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IEntitlementService entitlementService)
    : IRequestHandler<GetCurrentUserEntitlementsQuery, Result<PlanDto>>
{
    public async Task<Result<PlanDto>> Handle(GetCurrentUserEntitlementsQuery request, CancellationToken ct)
    {
        var userInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (userInfo is null)
            return Result<PlanDto>.Failure(GeneralErrors.NotFound);

        var rolesFromToken = currentUser.AppRoles ?? [];
        var entitlements = await entitlementService.GetEntitlementsAsync(userInfo.Id, rolesFromToken, ct);

        return Result<PlanDto>.Success(entitlements);
    }
}
