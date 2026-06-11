using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class ColumnRepository(JustTaskTrackerDbContext context)
    : Repository<Column, Guid>(context), IColumnRepository
{
    public async Task<Column?> GetByBoardIdAndIdAsync(Guid boardId, Guid columnId, CancellationToken ct = default) =>
        await _dbSet
            .FirstOrDefaultAsync(c => c.BoardId == boardId && c.Id == columnId, ct);

    public async Task<bool> ExistsByBoardIdAndIdAsync(Guid boardId, Guid columnId, CancellationToken ct = default) =>
        await _dbSet.AnyAsync(c => c.BoardId == boardId && c.Id == columnId, ct);

    public async Task<IReadOnlyList<string>> GetNameListByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(c => c.BoardId == boardId)
            .Select(c => c.Name)
            .ToListAsync(ct);

    public async Task<bool> NameExistsAsync(Guid boardId, string name, Guid? excludeColumnId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(c => c.BoardId == boardId);

        if (excludeColumnId.HasValue)
            query = query.Where(c => c.Id != excludeColumnId.Value);

        return await query.AnyAsync(
            c => c.Name.ToLower() == name.ToLower(),
            ct);
    }

    public async Task<IReadOnlyList<Column>> GetListWithPositionGreaterThanAsync(Guid boardId, int position, CancellationToken ct = default) =>
        await _dbSet
            .Where(c => c.BoardId == boardId && c.Position > position)
            .OrderBy(c => c.Position)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Column>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Position)
            .ToListAsync(ct);
}
