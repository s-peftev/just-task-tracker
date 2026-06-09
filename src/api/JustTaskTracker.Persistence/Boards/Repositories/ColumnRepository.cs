using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class ColumnRepository(JustTaskTrackerDbContext context)
    : Repository<Column, Guid>(context), IColumnRepository
{
    public async Task<IReadOnlyList<string>> GetNamesByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(c => c.BoardId == boardId)
            .Select(c => c.Name)
            .ToListAsync(ct);
}
