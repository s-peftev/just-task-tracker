using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Application.Billing.ReadModels;
using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Domain.Auth.Constants;
using JustTaskTracker.Domain.Billing.Constants;
using JustTaskTracker.Domain.Billing.DTOs;

namespace JustTaskTracker.Infrastructure.Billing;

internal class EntitlementService(
    IPlanCatalog planCatalog,
    ISubscriptionRepository subscriptionRepository) : IEntitlementService
{
    public async Task<bool> CanUseAsync(Guid userId, IReadOnlyList<string> globalRoles, string feature, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(feature);

        if (!Features.IsValid(feature))
            return false;

        var entitlements = await GetEntitlementsAsync(userId, globalRoles, ct);

        return entitlements.Features.Contains(feature);
    }

    public async Task<EntitlementDto> GetEntitlementsAsync(Guid userId, IReadOnlyList<string> globalRoles, CancellationToken ct = default)
    {
        var roles = globalRoles ?? [];
        var defaultPlan = planCatalog.GetPlan(planCatalog.DefaultPlanId);

        if (HasRole(roles, Roles.Admin))
        {
            return new EntitlementDto(
                defaultPlan.PlanId,
                defaultPlan.PlanDisplayName,
                SubscriptionStatus.Active,
                Features.GetAll().ToList());
        }

        var subscription = await subscriptionRepository.GetSubscriptionByUserIdAsync(userId, ct);

        return ResolveEntitlments(subscription);
    }

    public async Task<SubscriptionDetailsDto> GetUserSubscriptionAsync(Guid userId, CancellationToken ct = default)
    {
        var subscription = await subscriptionRepository.GetSubscriptionByUserIdAsync(userId, ct);

        if (subscription is null)
        {
            var defaultPlan = planCatalog.GetPlan(planCatalog.DefaultPlanId);

            return new SubscriptionDetailsDto(
                defaultPlan.PlanId,
                Status: SubscriptionStatus.Active,
                CancelAtPeriodEnd: false,
                HasBillableSubscription: false,
                CurrentPeriodStartUtc: null,
                CurrentPeriodEndUtc: null);
        }

        return new SubscriptionDetailsDto(
            subscription.PlanId,
            subscription.Status,
            subscription.CancelAtPeriodEnd,
            HasBillableSubscription: true,
            subscription.CurrentPeriodStartUtc,
            subscription.CurrentPeriodEndUtc);
    }

    public Task<string?> GetBillableStripeCustomerIdAsync(Guid userId, CancellationToken ct = default) =>
        subscriptionRepository.GetBillableStripeCustomerIdAsync(userId, ct);

    private EntitlementDto ResolveEntitlments(SubscriptionDetailsReadModel? subscription)
    {
        if (subscription is null)
            return BuildDefaultEntitlements();

        try
        {
            var effectivePlan = planCatalog.GetPlan(subscription.PlanId);

            return new EntitlementDto(
                effectivePlan.PlanId,
                effectivePlan.PlanDisplayName,
                subscription.Status,
                effectivePlan.Features);
        }
        catch (InvalidOperationException)
        {
            return BuildDefaultEntitlements();
        }
    }

    private EntitlementDto BuildDefaultEntitlements()
    {
        var defaultPlan = planCatalog.GetPlan(planCatalog.DefaultPlanId);

        return new EntitlementDto(
            defaultPlan.PlanId,
            defaultPlan.PlanDisplayName,
            SubscriptionStatus.Active,
            defaultPlan.Features);
    }

    private static bool HasRole(IReadOnlyList<string> globalRoles, string role) =>
        globalRoles.Contains(role, StringComparer.Ordinal);
}
