using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class ColumnRepository(JustTaskTrackerDbContext context)
    : Repository<Column, Guid>(context), IColumnRepository
{
    public async Task<(Column? Column, BoardMemberRole? UserRole)> GetColumnWithUserRoleAsync(Guid columnId, Guid azureAdObjectId, CancellationToken ct = default)
    {
        var result = await _dbSet
            .Where(c => c.Id == columnId)
            .Select(c => new
            {
                Column = c,
                UserRole = c.Board!.Members
                    .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
                    .Select(m => m.Role)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        return (result?.Column, result?.UserRole);
    }

    public async Task<BoardMemberRole?> GetUserRoleAsync(Guid columnId, Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet
            .Where(c => c.Id == columnId)
            .SelectMany(c => c.Board!.Members)
            .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<string>> GetNameListByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(c => c.BoardId == boardId)
            .Select(c => c.Name)
            .ToListAsync(ct);

    public async Task<bool> IsNameExistsAsync(Guid boardId, string name, Guid? excludeColumnId = null, CancellationToken ct = default)
    {
        var query = _dbSet.Where(c => c.BoardId == boardId);

        if (excludeColumnId.HasValue)
            query = query.Where(c => c.Id != excludeColumnId.Value);

        return await query.AnyAsync(
            c => c.Name.ToLower() == name.ToLower(),
            ct);
    }

    public async Task<IReadOnlyList<Column>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Position)
            .ToListAsync(ct);

    public async Task<int> CountByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet.CountAsync(c => c.BoardId == boardId, ct);

    public void RemoveRange(IReadOnlyList<Column> columns)
    {
        foreach (var column in columns)
            _dbSet.Remove(column);
    }
}
