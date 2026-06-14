using JustTaskTracker.Domain.Common.Interfaces;

namespace JustTaskTracker.Domain.Common;

public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>, ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
