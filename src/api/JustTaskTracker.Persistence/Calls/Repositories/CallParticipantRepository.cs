using JustTaskTracker.Application.Calls.ReadModels;
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

    public async Task<IReadOnlyList<CallParticipantHistoryReadModel>> GetParticipantHistoryAsync(Guid callSessionId, CancellationToken ct = default) =>
        await context.CallParticipants
            .Where(p => p.CallSessionId == callSessionId)
            .GroupBy(p => p.UserId)
            .Select(g => new CallParticipantHistoryReadModel(
                g.Key,
                g.Min(p => p.JoinedAtUtc),
                g.Max(p => (DateTime?)p.LeftAtUtc)))
            .ToListAsync(ct);

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>> GetActiveParticipantUserIdsForSessionsAsync(IReadOnlyList<Guid> callSessionIds, CancellationToken ct = default)
    {
        if (callSessionIds.Count is 0)
            return new Dictionary<Guid, IReadOnlyList<Guid>>();

        var participants = await context.CallParticipants
            .Where(p => callSessionIds.Contains(p.CallSessionId) && p.LeftAtUtc == null)
            .Select(p => new { p.CallSessionId, p.UserId })
            .ToListAsync(ct);

        return participants
            .GroupBy(x => x.CallSessionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Guid>)g.Select(x => x.UserId).ToList());
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<CallParticipantHistoryReadModel>>> GetParticipantHistoryForSessionsAsync(IReadOnlyList<Guid> callSessionIds, CancellationToken ct = default)
    {
        if (callSessionIds.Count is 0)
            return new Dictionary<Guid, IReadOnlyList<CallParticipantHistoryReadModel>>();

        var entries = await context.CallParticipants
            .Where(p => callSessionIds.Contains(p.CallSessionId))
            .GroupBy(p => new { p.CallSessionId, p.UserId })
            .Select(g => new
            {
                g.Key.CallSessionId,
                Entry = new CallParticipantHistoryReadModel(
                    g.Key.UserId,
                    g.Min(p => p.JoinedAtUtc),
                    g.Max(p => (DateTime?)p.LeftAtUtc))
            })
            .ToListAsync(ct);

        return entries
            .GroupBy(x => x.CallSessionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<CallParticipantHistoryReadModel>)g.Select(x => x.Entry).ToList());
    }

    public void Add(CallParticipant participant) =>
        context.CallParticipants.Add(participant);
}
