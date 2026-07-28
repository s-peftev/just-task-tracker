using JustTaskTracker.Application.Calls.ReadModels;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Domain.Common.Pagination;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallRepository : IRepository<CallSession, Guid>
{
    /// <summary>
    /// A single active session with its live state (participants/allowed users/linked tasks/
    /// creator) already projected via navigation properties -- for building a session's response
    /// DTO right after it's created or joined, without a separate round trip per dependency.
    /// </summary>
    Task<CallSessionWithStateReadModel?> GetSessionWithStateAsync(Guid callSessionId, CancellationToken ct = default);

    /// <summary>
    /// Every active session on a board with its live state already projected via navigation
    /// properties, in one query (AD-2/AD-10) -- backs Story 3.2's board-page live state, not one
    /// round trip per session.
    /// </summary>
    Task<IReadOnlyList<CallSessionWithStateReadModel>> GetActiveSessionsWithStateForBoardAsync(Guid boardId, CancellationToken ct = default);

    /// <summary>
    /// A page of a board's closed sessions with their history state (linked tasks/participant
    /// events/allowed users/creator) already projected via navigation properties, in one query.
    /// </summary>
    Task<PagedList<CallSessionHistoryReadModel>> GetClosedSessionsWithStateForBoardAsync(Guid boardId, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<CallSession?> GetByAcsRoomIdAsync(string acsRoomId, CancellationToken ct = default);

    /// <summary>
    /// Atomically claims the presenter slot for <paramref name="userId"/> -- a single conditional
    /// UPDATE (AD-9), never a read-then-write, so two concurrent requests can't both win.
    /// </summary>
    /// <returns><see langword="true"/> if the slot was free and now belongs to this user.</returns>
    Task<bool> TryAcquirePresenterAsync(Guid callSessionId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Atomically releases the presenter slot, only if <paramref name="userId"/> currently holds it.
    /// </summary>
    /// <returns><see langword="true"/> if this user was the presenter and the slot is now free.</returns>
    Task<bool> TryReleasePresenterAsync(Guid callSessionId, Guid userId, CancellationToken ct = default);
}
