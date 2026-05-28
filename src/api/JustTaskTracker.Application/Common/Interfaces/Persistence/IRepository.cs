namespace JustTaskTracker.Application.Common.Interfaces.Persistence;

public interface IRepository<TEntity, TKey>
        where TEntity : class
{
    TEntity Add(TEntity entity);
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<bool> RemoveAsync(TKey id, CancellationToken ct = default);
    TEntity Update(TEntity entity);
}
