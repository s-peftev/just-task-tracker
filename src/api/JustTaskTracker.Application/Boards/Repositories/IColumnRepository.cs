using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Entities;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IColumnRepository : IRepository<Column, Guid>
{
    Task<Column?> GetByBoardIdAndIdAsync(Guid boardId, Guid columnId, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetNamesByBoardIdAsync(Guid boardId, CancellationToken ct = default);

    Task<bool> NameExistsAsync(
        Guid boardId,
        string name,
        Guid? excludeColumnId = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Column>> GetWithPositionGreaterThanAsync(
        Guid boardId,
        int position,
        CancellationToken ct = default);
}
