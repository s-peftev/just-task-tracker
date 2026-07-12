using JustTaskTracker.Application.Billing.ReadModels;
using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Domain.Billing.Constants;
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
}
