using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Billing.Queries;

public record GetCurrentUserSubscriptionQuery : IRequest<Result<SubscriptionDetailsDto>>;

public class GetCurrentUserSubscriptionQueryHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IEntitlementService entitlementService)
    : IRequestHandler<GetCurrentUserSubscriptionQuery, Result<SubscriptionDetailsDto>>
{
    public async Task<Result<SubscriptionDetailsDto>> Handle(
        GetCurrentUserSubscriptionQuery request,
        CancellationToken ct)
    {
        var userInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (userInfo is null)
            return Result<SubscriptionDetailsDto>.Failure(GeneralErrors.NotFound);

        var subscription = await entitlementService.GetUserSubscriptionAsync(userInfo.Id, ct);

        return Result<SubscriptionDetailsDto>.Success(subscription);
    }
}
