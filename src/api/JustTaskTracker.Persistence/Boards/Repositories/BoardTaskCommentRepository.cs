using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Extentions;
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

    public async Task<PagedList<BoardTaskCommentDto>> GetPagedByBoardIdAndColumnIdAndTaskIdAsync(
        Guid boardId,
        Guid columnId,
        Guid boardTaskId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default) =>
        await _dbSet
            .Where(comment => comment.BoardTaskId == boardTaskId
                && comment.BoardTask!.ColumnId == columnId
                && comment.BoardTask.Column!.BoardId == boardId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ThenBy(comment => comment.Id)
            .ToPagedAsync(
                comment => new BoardTaskCommentDto(
                    comment.Id,
                    comment.Body,
                    comment.CreatedAtUtc,
                    new UserDto(
                        comment.Author!.Id,
                        comment.Author.Email,
                        comment.Author.DisplayName),
                    comment.LastModifiedAtUtc),
                pageNumber,
                pageSize,
                ct);

    public async Task<BoardTaskComment?> GetByBoardIdAndColumnIdAndTaskIdAndIdAsync(
        Guid boardId,
        Guid columnId,
        Guid boardTaskId,
        Guid commentId,
        CancellationToken ct = default) =>
        await _dbSet
            .FirstOrDefaultAsync(
                comment => comment.Id == commentId
                    && comment.BoardTaskId == boardTaskId
                    && comment.BoardTask!.ColumnId == columnId
                    && comment.BoardTask.Column!.BoardId == boardId,
                ct);

    public void RemoveRange(IReadOnlyList<BoardTaskComment> comments)
    {
        foreach (var comment in comments)
            _dbSet.Remove(comment);
    }
}
