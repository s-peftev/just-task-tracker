using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Persistence.Common.Constants;
using Microsoft.EntityFrameworkCore.Storage;

namespace JustTaskTracker.Persistence.Common;

/// <summary>
/// Transaction scope for a single <see cref="JustTaskTrackerDbContext"/> instance. <see cref="BeginTransactionAsync"/> is idempotent while a transaction is open;
/// commit/rollback always dispose the underlying transaction so a subsequent begin starts fresh.
/// </summary>
public class UnitOfWork(JustTaskTrackerDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    /// <summary>No-op if a transaction is already active (nested callers do not start a second transaction).</summary>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            return;

        _transaction = await context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException(UoWExceptionMessages.NoActiveTransaction);

        try
        {
            await _transaction.CommitAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            return;

        try
        {
            await _transaction.RollbackAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
