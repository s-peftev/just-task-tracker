namespace JustTaskTracker.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; init; } = default!;
}
