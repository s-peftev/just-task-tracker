using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IColumnRepository : IRepository<Column, Guid>
{
    Task<(Column? Column, BoardMemberRole? UserRole)> GetColumnWithUserRoleAsync(Guid columnId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardMemberRole?> GetUserRoleAsync(Guid columnId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetNameListByBoardIdAsync(Guid boardId, CancellationToken ct = default);

    Task<bool> IsNameExistsAsync(Guid boardId, string name, Guid? excludeColumnId = null, CancellationToken ct = default);

    Task<IReadOnlyList<Column>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default);

    void RemoveRange(IReadOnlyList<Column> columns);
}
