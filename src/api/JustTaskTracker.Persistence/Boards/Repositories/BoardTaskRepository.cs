using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class BoardTaskRepository(JustTaskTrackerDbContext context)
    : Repository<BoardTask, Guid>(context), IBoardTaskRepository
{
    public async Task<BoardTask?> GetByBoardIdAndIdAsync(Guid boardId, Guid boardTaskId, CancellationToken ct = default) =>
        await _dbSet
            .FirstOrDefaultAsync(
                task => task.Id == boardTaskId && task.Column!.BoardId == boardId,
                ct);

    public async Task<IReadOnlyList<BoardTask>> GetOrderedByColumnIdAsync(Guid columnId, CancellationToken ct = default) =>
        await _dbSet
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Position)
            .ToListAsync(ct);

    public async Task<int> GetCountByColumnIdAsync(Guid columnId, CancellationToken ct = default) =>
        await _dbSet.CountAsync(t => t.ColumnId == columnId, ct);

    public void RemoveRange(IReadOnlyList<BoardTask> tasks)
    {
        foreach (var task in tasks)
            _dbSet.Remove(task);
    }
}
