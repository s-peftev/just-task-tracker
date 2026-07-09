using JustTaskTracker.Domain.Billing.Entities;

namespace JustTaskTracker.Application.Billing.Repositories;

public interface ISubscriptionRepository
{
    /// <summary>
    /// Returns the user's current billable subscription, if any
    /// (<see cref="Domain.Billing.Enums.SubscriptionStatus.IsBillable"/>).
    /// </summary>
    Task<Subscription?> GetBillableByUserIdAsync(Guid userId, CancellationToken ct = default);
}
