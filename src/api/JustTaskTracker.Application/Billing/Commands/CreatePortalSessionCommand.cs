using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Billing.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Billing.Commands;

public record CreatePortalSessionCommand : IRequest<Result<PortalSessionResult>>;

public class CreatePortalSessionCommandHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IEntitlementService entitlementService,
    IBillingService billingService,
    ILogger<CreatePortalSessionCommandHandler> logger)
    : IRequestHandler<CreatePortalSessionCommand, Result<PortalSessionResult>>
{
    public async Task<Result<PortalSessionResult>> Handle(
        CreatePortalSessionCommand request,
        CancellationToken ct)
    {
        var userInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (userInfo is null)
            return Result<PortalSessionResult>.Failure(GeneralErrors.NotFound);

        var stripeCustomerId = await entitlementService.GetBillableStripeCustomerIdAsync(userInfo.Id, ct);

        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            return Result<PortalSessionResult>.Failure(BillingErrors.SubscriptionNotFound);

        try
        {
            var portalUrl = await billingService.CreateCustomerPortalSessionAsync(stripeCustomerId, ct);

            return Result<PortalSessionResult>.Success(new PortalSessionResult(portalUrl));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create Stripe Customer Portal Session for user {UserId}.",
                userInfo.Id);

            return Result<PortalSessionResult>.Failure(GeneralErrors.ServiceUnavailable);
        }
    }
}
