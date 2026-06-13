using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs;
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

    public async Task<BoardTaskDetailsDto?> GetDetailsByBoardIdAndColumnIdAndIdAsync(
        Guid boardId,
        Guid columnId,
        Guid boardTaskId,
        CancellationToken ct = default) =>
        await _dbSet
            .Where(task => task.Id == boardTaskId
                && task.ColumnId == columnId
                && task.Column!.BoardId == boardId)
            .Select(task => new BoardTaskDetailsDto(
                task.Id,
                task.ColumnId,
                task.Column!.Name,
                task.Title,
                task.Position,
                task.CreatedAtUtc,
                new UserDto(task.Reporter!.Id, task.Reporter.Email, task.Reporter.DisplayName),
                default,
                task.Description,
                task.Assignee == null
                    ? null
                    : new UserDto(task.Assignee.Id, task.Assignee.Email, task.Assignee.DisplayName)))
            .FirstOrDefaultAsync(ct);

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
