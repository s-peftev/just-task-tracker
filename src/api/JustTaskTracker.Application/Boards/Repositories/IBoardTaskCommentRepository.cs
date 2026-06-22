using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Pagination;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IBoardTaskCommentRepository : IRepository<BoardTaskComment, Guid>
{
    Task<IReadOnlyList<BoardTaskComment>> GetListByBoardTaskIdAsync(Guid boardTaskId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTaskComment>> GetListByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTaskComment>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default);

    Task<(BoardTaskComment? BoardTaskComment, BoardMemberRole? UserRole)> GetBoardTaskCommentWithUserRole(Guid boardTaskCommentId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<PagedList<BoardTaskCommentReadModel>> GetPagedInfoByBoardTaskIdAsync(Guid boardTaskId, int pageNumber, int pageSize, CancellationToken ct = default);

    void RemoveRange(IReadOnlyList<BoardTaskComment> comments);
}
