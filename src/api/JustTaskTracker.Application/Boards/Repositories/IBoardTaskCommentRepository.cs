using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Entities;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IBoardTaskCommentRepository : IRepository<BoardTaskComment, Guid>
{
    Task<IReadOnlyList<BoardTaskComment>> GetOrderedByBoardTaskIdAsync(Guid boardTaskId, CancellationToken ct = default);

    void RemoveRange(IReadOnlyList<BoardTaskComment> comments);
}
