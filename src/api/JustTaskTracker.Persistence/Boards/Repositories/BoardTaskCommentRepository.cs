using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Extentions;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class BoardTaskCommentRepository(JustTaskTrackerDbContext context)
    : Repository<BoardTaskComment, Guid>(context), IBoardTaskCommentRepository
{
    public async Task<IReadOnlyList<BoardTaskComment>> GetListByBoardTaskIdAsync(Guid boardTaskId, CancellationToken ct = default) =>
        await _dbSet
            .Where(comment => comment.BoardTaskId == boardTaskId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ThenBy(comment => comment.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BoardTaskComment>> GetListByColumnIdAsync(Guid columnId, CancellationToken ct = default) =>
        await _dbSet
            .Where(comment => comment.BoardTask!.ColumnId == columnId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ThenBy(comment => comment.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BoardTaskComment>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(comment => comment.BoardTask!.Column!.BoardId == boardId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ThenBy(comment => comment.Id)
            .ToListAsync(ct);

    public async Task<(BoardTaskComment? BoardTaskComment, BoardMemberRole? UserRole)> GetBoardTaskCommentWithUserRole(Guid boardTaskCommentId, Guid azureAdObjectId, CancellationToken ct = default)
    {
        var result = await _dbSet
            .Where(comment => comment.Id == boardTaskCommentId)
            .Select(comment => new
            {
                BoardTaskComment = comment,
                UserRole = comment.BoardTask!.Column!.Board!.Members
                    .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
                    .Select(m => m.Role)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        return (result?.BoardTaskComment, result?.UserRole);
    }

    public async Task<PagedList<BoardTaskCommentDto>> GetPagedByBoardTaskIdAsync(Guid boardTaskId, int pageNumber, int pageSize, CancellationToken ct = default) =>
        await _dbSet
            .Where(comment => comment.BoardTaskId == boardTaskId)
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

    public void RemoveRange(IReadOnlyList<BoardTaskComment> comments)
    {
        foreach (var comment in comments)
            _dbSet.Remove(comment);
    }
}
