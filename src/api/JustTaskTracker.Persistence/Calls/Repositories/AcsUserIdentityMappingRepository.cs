using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Calls.Repositories;

public class AcsUserIdentityMappingRepository(JustTaskTrackerDbContext context) : IAcsUserIdentityMappingRepository
{
    public Task<AcsUserIdentityMapping?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        context.AcsUserIdentityMappings.FirstOrDefaultAsync(m => m.UserId == userId, ct);

    public void Add(AcsUserIdentityMapping mapping) =>
        context.AcsUserIdentityMappings.Add(mapping);
}
