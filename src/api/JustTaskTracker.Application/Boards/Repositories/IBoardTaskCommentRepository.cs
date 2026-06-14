using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Common.Pagination;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IBoardTaskCommentRepository : IRepository<BoardTaskComment, Guid>
{
    Task<IReadOnlyList<BoardTaskComment>> GetOrderedByBoardTaskIdAsync(Guid boardTaskId, CancellationToken ct = default);

    Task<PagedList<BoardTaskCommentDto>> GetPagedByBoardIdAndColumnIdAndTaskIdAsync(
        Guid boardId,
        Guid columnId,
        Guid boardTaskId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<BoardTaskComment?> GetByBoardIdAndColumnIdAndTaskIdAndIdAsync(
        Guid boardId,
        Guid columnId,
        Guid boardTaskId,
        Guid commentId,
        CancellationToken ct = default);

    void RemoveRange(IReadOnlyList<BoardTaskComment> comments);
}
