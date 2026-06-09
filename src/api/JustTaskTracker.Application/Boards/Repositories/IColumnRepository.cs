using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Entities;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IColumnRepository : IRepository<Column, Guid>
{
    Task<IReadOnlyList<string>> GetNamesByBoardIdAsync(Guid boardId, CancellationToken ct = default);
}
