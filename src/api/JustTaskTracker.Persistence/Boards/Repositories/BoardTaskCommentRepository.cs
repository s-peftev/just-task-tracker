using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class BoardTaskCommentRepository(JustTaskTrackerDbContext context)
    : Repository<BoardTaskComment, Guid>(context), IBoardTaskCommentRepository
{
    public async Task<IReadOnlyList<BoardTaskComment>> GetOrderedByBoardTaskIdAsync(
        Guid boardTaskId,
        CancellationToken ct = default) =>
        await _dbSet
            .Where(comment => comment.BoardTaskId == boardTaskId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ThenBy(comment => comment.Id)
            .ToListAsync(ct);

    public void RemoveRange(IReadOnlyList<BoardTaskComment> comments)
    {
        foreach (var comment in comments)
            _dbSet.Remove(comment);
    }
}
