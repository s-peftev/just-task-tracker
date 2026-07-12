using JustTaskTracker.Application.Billing.ReadModels;
using JustTaskTracker.Domain.Billing.Constants;

namespace JustTaskTracker.Application.Billing.Repositories;

public interface ISubscriptionRepository
{
    /// <summary>
    /// Returns the plan id of the user's current billable subscription, if any
    /// (<see cref="SubscriptionStatus.IsBillable"/>);
    /// otherwise <see langword="null"/>.
    /// </summary>
    Task<string?> GetUserPlanIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns details of the user's current billable subscription, if any;
    /// otherwise <see langword="null"/>.
    /// </summary>
    Task<SubscriptionDetailsReadModel?> GetSubscriptionByUserIdAsync(Guid userId, CancellationToken ct = default);
}
