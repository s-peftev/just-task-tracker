using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Calls.Repositories;

public class CallRepository(JustTaskTrackerDbContext context) : Repository<CallSession, Guid>(context), ICallRepository
{
    public async Task<IReadOnlyList<CallSession>> GetActiveSessionsForBoardAsync(Guid boardId, CancellationToken ct = default) =>
        await _context.CallSessions
            .Where(s => s.BoardId == boardId && s.Status == CallStatus.Active)
            .ToListAsync(ct);
}
