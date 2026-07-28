using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallSessionLinkedTaskRepository
{
    Task<IReadOnlyList<Guid>> GetLinkedTaskIdsAsync(Guid callSessionId, CancellationToken ct = default);

    void Add(CallSessionLinkedTask linkedTask);
}
