using FluentValidation;
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

public record CreateCheckoutSessionCommand(string PlanId) : IRequest<Result<CheckoutSessionResult>>;

public class CreateCheckoutSessionCommandHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IEntitlementService entitlementService,
    IBillingService billingService,
    ILogger<CreateCheckoutSessionCommandHandler> logger)
    : IRequestHandler<CreateCheckoutSessionCommand, Result<CheckoutSessionResult>>
{
    public async Task<Result<CheckoutSessionResult>> Handle(CreateCheckoutSessionCommand request, CancellationToken ct)
    {
        var userInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (userInfo is null)
            return Result<CheckoutSessionResult>.Failure(GeneralErrors.NotFound);

        var subscription = await entitlementService.GetUserSubscriptionAsync(userInfo.Id, ct);

        if (subscription.HasBillableSubscription)
            return Result<CheckoutSessionResult>.Failure(BillingErrors.SubscriptionAlreadyExists);

        try
        {
            var session = await billingService.CreateCheckoutSessionAsync(
                userInfo.Id,
                userInfo.Email,
                request.PlanId,
                stripeCustomerId: null,
                ct);

            return Result<CheckoutSessionResult>.Success(session);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to create Stripe Checkout Session for user {UserId} and plan {PlanId}.",
                userInfo.Id,
                request.PlanId);

            return Result<CheckoutSessionResult>.Failure(GeneralErrors.ServiceUnavailable);
        }
    }
}

public class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator(IPlanCatalog planCatalog)
    {
        RuleFor(x => x.PlanId)
            .Must(planId => !string.IsNullOrWhiteSpace(planId))
            .WithMessage("'PlanId' must not be empty.")
            .DependentRules(() =>
            {
                RuleFor(x => x.PlanId)
                    .Must(planId => IsKnownPlan(planCatalog, planId))
                    .WithMessage(x => $"Plan '{x.PlanId}' is not available.")
                    .Must(planId => HasCheckoutPrice(planCatalog, planId))
                    .WithMessage(x => $"Plan '{x.PlanId}' is not available for checkout.");
            });
    }

    private static bool IsKnownPlan(IPlanCatalog planCatalog, string planId) =>
        planCatalog.GetAllPlans()
            .Any(plan => plan.PlanId.Equals(planId, StringComparison.OrdinalIgnoreCase));

    private static bool HasCheckoutPrice(IPlanCatalog planCatalog, string planId) =>
        !string.IsNullOrWhiteSpace(planCatalog.TryGetPriceId(planId));
}
