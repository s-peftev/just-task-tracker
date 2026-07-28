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

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>> GetAllowedUserIdsForSessionsAsync(IReadOnlyList<Guid> callSessionIds, CancellationToken ct = default)
    {
        if (callSessionIds.Count is 0)
            return new Dictionary<Guid, IReadOnlyList<Guid>>();

        var allowed = await context.CallSessionAllowedParticipants
            .Where(p => callSessionIds.Contains(p.CallSessionId))
            .Select(p => new { p.CallSessionId, p.UserId })
            .ToListAsync(ct);

        return allowed
            .GroupBy(x => x.CallSessionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Guid>)g.Select(x => x.UserId).ToList());
    }

    public void Add(CallSessionAllowedParticipant allowedParticipant) =>
        context.CallSessionAllowedParticipants.Add(allowedParticipant);
}
