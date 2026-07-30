using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Domain.Common.Pagination;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallRepository : IRepository<CallSession, Guid>
{
    Task<IReadOnlyList<CallSession>> GetActiveSessionsForBoardAsync(Guid boardId, CancellationToken ct = default);

    Task<PagedList<CallSession>> GetClosedSessionsForBoardAsync(Guid boardId, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<CallSession?> GetByAcsRoomIdAsync(string acsRoomId, CancellationToken ct = default);
}
