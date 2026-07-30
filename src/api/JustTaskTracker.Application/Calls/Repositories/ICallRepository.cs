using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Domain.Common.Pagination;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallRepository : IRepository<CallSession, Guid>
{
    Task<IReadOnlyList<CallSession>> GetActiveSessionsForBoardAsync(Guid boardId, CancellationToken ct = default);

    Task<PagedList<CallSession>> GetClosedSessionsForBoardAsync(Guid boardId, int pageNumber, int pageSize, CancellationToken ct = default);

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
