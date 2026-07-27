using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Extentions;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Calls.Repositories;

public class CallRepository(JustTaskTrackerDbContext context) : Repository<CallSession, Guid>(context), ICallRepository
{
    public async Task<IReadOnlyList<CallSession>> GetActiveSessionsForBoardAsync(Guid boardId, CancellationToken ct = default) =>
        await _context.CallSessions
            .Where(s => s.BoardId == boardId && s.Status == CallStatus.Active)
            .ToListAsync(ct);

    public Task<PagedList<CallSession>> GetClosedSessionsForBoardAsync(Guid boardId, int pageNumber, int pageSize, CancellationToken ct = default) =>
        _context.CallSessions
            .Where(s => s.BoardId == boardId && s.Status == CallStatus.Closed)
            .OrderByDescending(s => s.EndedAtUtc)
            .ToPagedAsync(pageNumber, pageSize, ct);

    public Task<CallSession?> GetByAcsRoomIdAsync(string acsRoomId, CancellationToken ct = default) =>
        _context.CallSessions.FirstOrDefaultAsync(s => s.AcsRoomId == acsRoomId, ct);
}
