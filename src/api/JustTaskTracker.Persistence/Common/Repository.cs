using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Common;

public abstract class Repository<TEntity, TId> : IRepository<TEntity, TId>
        where TEntity : BaseEntity<TId>
{
    protected readonly JustTaskTrackerDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(JustTaskTrackerDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public virtual TEntity Add(TEntity entity)
    {
        _dbSet.Add(entity);
        return entity;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default) =>
        await _dbSet.FindAsync([id], cancellationToken: ct);

    public virtual void Remove(TEntity entity) =>
        _dbSet.Remove(entity);

    public virtual async Task<bool> RemoveByIdAsync(TId id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);

        if (entity is null)
            return false;

        _dbSet.Remove(entity);

        return true;
    }

    public virtual TEntity Update(TEntity entity)
    {
        _dbSet.Update(entity);
        return entity;
    }
}
