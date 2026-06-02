using JustTaskTracker.Application.Common.Interfaces.Persistence;

namespace JustTaskTracker.Persistence.Common;

public class UnitOfWork(JustTaskTrackerDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}
