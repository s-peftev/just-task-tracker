using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Common.Entities;

namespace JustTaskTracker.Domain.Billing.Entities;

public class Subscription : AuditableEntity<Guid>
{
    public required Guid UserId { get; set; }
    public required string PlanId { get; set; }
    public required string StripeCustomerId { get; set; }
    public required string StripeSubscriptionId { get; set; }
    public required string Status { get; set; }
    public DateTime? CurrentPeriodStartUtc { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public bool CancelAtPeriodEnd { get; set; }

    public User? User { get; set; }
}
