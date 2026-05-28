using JustTaskTracker.Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence;

public abstract class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }
    public virtual TEntity Add(TEntity entity)
    {
        _dbSet.Add(entity);

        return entity;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken: ct);
    }

    public virtual async Task<bool> RemoveAsync(TKey id, CancellationToken ct = default)
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
