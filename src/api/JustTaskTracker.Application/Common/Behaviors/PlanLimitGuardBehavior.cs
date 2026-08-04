using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Billing.Enums;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Common.Behaviors;

/// <summary>
/// Marks a MediatR request that must stay within the effective plan limit
/// before the handler runs.
/// </summary>
/// <remarks>
/// <see cref="PlanLimitKind.Boards"/> is evaluated against the actor's plan.
/// Board-scoped limits use the board owner's plan.
/// </remarks>
public interface IRequirePlanLimit
{
    PlanLimitKind Limit { get; }

    /// <summary>
    /// Target board for board-scoped limits; <see langword="null"/> for <see cref="PlanLimitKind.Boards"/>.
    /// </summary>
    Guid? BoardId { get; }
}

/// <summary>
/// MediatR pipeline behavior that blocks create/add requests that would exceed
/// the effective plan limit. Applies only to requests implementing <see cref="IRequirePlanLimit"/>.
/// </summary>
public class PlanLimitGuardBehavior<TRequest, TResponse>(
    IPlanLimitChecker planLimitChecker,
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
        if (request is not IRequirePlanLimit limitRequest)
            return await next(ct);

        var user = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (user is null)
            return ResultResponseFactory.CreateFailure<TResponse>(GeneralErrors.Unauthorized);

        var error = await planLimitChecker.EvaluateAsync(
            limitRequest.Limit,
            user.Id,
            limitRequest.BoardId,
            ct);

        if (error is not null)
            return ResultResponseFactory.CreateFailure<TResponse>(error);

        return await next(ct);
    }
}
