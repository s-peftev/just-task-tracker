using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Calls.Repositories;

public class CallParticipantRepository(JustTaskTrackerDbContext context) : ICallParticipantRepository
{
    public Task<CallParticipant?> GetActiveParticipantAsync(Guid callSessionId, Guid userId, CancellationToken ct = default) =>
        context.CallParticipants
            .FirstOrDefaultAsync(p => p.CallSessionId == callSessionId && p.UserId == userId && p.LeftAtUtc == null, ct);

    public async Task<IReadOnlyList<CallParticipant>> GetActiveParticipantsAsync(Guid callSessionId, CancellationToken ct = default) =>
        await context.CallParticipants
            .Where(p => p.CallSessionId == callSessionId && p.LeftAtUtc == null)
            .ToListAsync(ct);

    public void Add(CallParticipant participant) =>
        context.CallParticipants.Add(participant);
}
