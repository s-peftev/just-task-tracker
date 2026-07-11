namespace JustTaskTracker.Application.Billing.Repositories;

public interface ISubscriptionRepository
{
    /// <summary>
    /// Returns the plan id of the user's current billable subscription, if any
    /// (<see cref="Domain.Billing.Enums.SubscriptionStatus.IsBillable"/>);
    /// otherwise <see langword="null"/>.
    /// </summary>
    Task<string?> GetUserPlanIdAsync(Guid userId, CancellationToken ct = default);
}
