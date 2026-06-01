using JustTaskTracker.Domain.Common.Interfaces;

namespace JustTaskTracker.Domain.Common;

public abstract class BaseEntity<TId> : IAuditable, ISoftDeletable
{
    public TId Id { get; init; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
