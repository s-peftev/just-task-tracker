using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Entities;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IBoardTaskRepository : IRepository<BoardTask, Guid>
{
    Task<IReadOnlyList<BoardTask>> GetOrderedByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    Task<int> GetCountByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    void RemoveRange(IReadOnlyList<BoardTask> tasks);
}
