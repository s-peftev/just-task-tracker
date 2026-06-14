using JustTaskTracker.Domain.Common.Interfaces;

namespace JustTaskTracker.Domain.Common;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
