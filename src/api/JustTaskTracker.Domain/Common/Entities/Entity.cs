namespace JustTaskTracker.Domain.Common.Entities;

public abstract class Entity<TId>
{
    public TId Id { get; init; } = default!;
}
