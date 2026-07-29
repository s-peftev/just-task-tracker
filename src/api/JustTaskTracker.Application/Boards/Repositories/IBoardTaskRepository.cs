using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Searching;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IBoardTaskRepository : IRepository<BoardTask, Guid>
{
    Task<(BoardTask? BoardTask, BoardMemberRole? UserRole)> GetBoardTaskWithUserRoleAsync(Guid boardTaskId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardMemberRole?> GetUserRoleAsync(Guid boardTaskId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<bool> ExistsInBoardAsync(Guid boardTaskId, Guid boardId, CancellationToken ct = default);

    /// <summary>
    /// How many of <paramref name="boardTaskIds"/> exist on the board -- lets a caller validate an
    /// entire id list in one round trip (compare against the distinct id count) instead of one
    /// <see cref="ExistsInBoardAsync"/> call per id.
    /// </summary>
    Task<int> CountExistingInBoardAsync(Guid boardId, IReadOnlyList<Guid> boardTaskIds, CancellationToken ct = default);

    Task<BoardTaskDetailsReadModel?> GetBoardTaskDetailsAsync(Guid boardTaskId, CancellationToken ct = default);

    Task<PagedList<BoardTaskLookupDto>> GetBoardTaskLookupListAsync(Guid boardId, int pageNumber, int pageSize, TextSearchOptions<BoardTaskSearchField>? searchOptions = null, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTask>> GetListByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTask>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default);

    Task<int> GetCountByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    Task<int> CountByBoardIdAsync(Guid boardId, CancellationToken ct = default);

    void RemoveRange(IReadOnlyList<BoardTask> tasks);
}
