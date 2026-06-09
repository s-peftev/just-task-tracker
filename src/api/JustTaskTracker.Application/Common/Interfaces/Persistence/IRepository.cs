using JustTaskTracker.Domain.Common;

namespace JustTaskTracker.Application.Common.Interfaces.Persistence;

public interface IRepository<TEntity, TId>
        where TEntity : BaseEntity<TId>
{
    TEntity Add(TEntity entity);
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    void Remove(TEntity entity);
    Task<bool> RemoveByIdAsync(TId id, CancellationToken ct = default);
    TEntity Update(TEntity entity);
}
