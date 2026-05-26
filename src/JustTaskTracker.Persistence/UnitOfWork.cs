using JustTaskTracker.Application.Common.Interfaces.Persistence;

namespace JustTaskTracker.Persistence;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
