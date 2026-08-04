namespace JustTaskTracker.Domain.Assistant.ReadModels;

/// <summary>
/// Keyless projection of <c>vw_Assistant_RequesterAccount</c>:
/// global roles and billable subscription snapshot for the current user.
/// </summary>
public class AssistantRequesterAccountReadModel
{
    public Guid UserId { get; set; }

    /// <summary>
    /// Comma-aggregated global roles (e.g. <c>ADMIN,USER</c>), or empty when none.
    /// </summary>
    public required string GlobalRoles { get; set; }

    public bool HasBillableSubscription { get; set; }

    public string? PlanId { get; set; }

    public string? SubscriptionStatus { get; set; }

    public bool? CancelAtPeriodEnd { get; set; }

    public DateTime? CurrentPeriodStartUtc { get; set; }

    public DateTime? CurrentPeriodEndUtc { get; set; }
}
