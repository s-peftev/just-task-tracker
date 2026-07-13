using JustTaskTracker.Application.Billing.ReadModels;
using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Domain.Billing.Constants;
using JustTaskTracker.Domain.Billing.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Billing.Repositories;

public class SubscriptionRepository(JustTaskTrackerDbContext context) : ISubscriptionRepository
{
    public Task<string?> GetUserPlanIdAsync(Guid userId, CancellationToken ct = default) =>
        context.Subscriptions
            .Where(s => s.UserId == userId && SubscriptionStatus.AllBillable.Contains(s.Status))
            .Select(s => s.PlanId)
            .FirstOrDefaultAsync(ct);

    public Task<SubscriptionDetailsReadModel?> GetSubscriptionByUserIdAsync(
        Guid userId,
        CancellationToken ct = default) =>
        context.Subscriptions
            .Where(s => s.UserId == userId && SubscriptionStatus.AllBillable.Contains(s.Status))
            .Select(s => new SubscriptionDetailsReadModel(
                s.PlanId,
                s.Status,
                s.CancelAtPeriodEnd,
                s.CurrentPeriodStartUtc,
                s.CurrentPeriodEndUtc))
            .FirstOrDefaultAsync(ct);

    public Task<bool> ExistsByStripeSubscriptionIdAsync(
        string stripeSubscriptionId,
        CancellationToken ct = default) =>
        context.Subscriptions.AnyAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, ct);

    public Task<Subscription?> GetByStripeSubscriptionIdAsync(
        string stripeSubscriptionId,
        CancellationToken ct = default) =>
        context.Subscriptions.FirstOrDefaultAsync(
            s => s.StripeSubscriptionId == stripeSubscriptionId,
            ct);

    public void Add(Subscription subscription) =>
        context.Subscriptions.Add(subscription);
}
