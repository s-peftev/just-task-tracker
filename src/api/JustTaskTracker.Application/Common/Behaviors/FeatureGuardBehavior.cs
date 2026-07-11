using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Billing.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that blocks requests when the current user
/// is not entitled to the required billing feature.
/// Applies only to requests implementing <see cref="IRequireFeature"/>.
/// </summary>
public class FeatureGuardBehavior<TRequest, TResponse>(
    IEntitlementService entitlementService,
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not IRequireFeature featureRequest)
            return await next(ct);

        var user = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (user is null)
            return ResultResponseFactory.CreateFailure<TResponse>(GeneralErrors.Unauthorized);

        if (await entitlementService.CanUseAsync(
                user.Id,
                currentUserAccessor.AppRoles,
                featureRequest.Feature,
                ct))
        {
            return await next(ct);
        }

        return ResultResponseFactory.CreateFailure<TResponse>(EntitlementErrors.FeatureNotAvailable);
    }
}
