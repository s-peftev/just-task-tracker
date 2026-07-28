using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallSessionLinkedTaskRepository
{
    Task<IReadOnlyList<Guid>> GetLinkedTaskIdsAsync(Guid callSessionId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTaskLookupDto>> GetLinkedTaskLookupsAsync(Guid callSessionId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, IReadOnlyList<BoardTaskLookupDto>>> GetLinkedTaskLookupsForSessionsAsync(IReadOnlyList<Guid> callSessionIds, CancellationToken ct = default);

    void Add(CallSessionLinkedTask linkedTask);
}
