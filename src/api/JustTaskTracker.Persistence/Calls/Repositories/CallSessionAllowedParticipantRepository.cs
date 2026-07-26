using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Calls.Repositories;

public class CallSessionAllowedParticipantRepository(JustTaskTrackerDbContext context) : ICallSessionAllowedParticipantRepository
{
    public Task<bool> IsAllowedAsync(Guid callSessionId, Guid userId, CancellationToken ct = default) =>
        context.CallSessionAllowedParticipants
            .AnyAsync(p => p.CallSessionId == callSessionId && p.UserId == userId, ct);

    public async Task<IReadOnlyList<Guid>> GetAllowedUserIdsAsync(Guid callSessionId, CancellationToken ct = default) =>
        await context.CallSessionAllowedParticipants
            .Where(p => p.CallSessionId == callSessionId)
            .Select(p => p.UserId)
            .ToListAsync(ct);

    public void Add(CallSessionAllowedParticipant allowedParticipant) =>
        context.CallSessionAllowedParticipants.Add(allowedParticipant);
}
