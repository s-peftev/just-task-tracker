using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Helpers;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Boards.DTOs.Columns;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Searching;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Extentions;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class BoardRepository(JustTaskTrackerDbContext context)
    : Repository<Board, Guid>(context), IBoardRepository
{
    public void AddMember(BoardMember member) =>
        _context.BoardMembers.Add(member);

    public async Task<BoardMemberRole?> GetUserRoleAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet
            .Where(b => b.Id == boardId)
            .SelectMany(b => b.Members)
            .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(ct);

    public async Task<(Board? Board, BoardMemberRole? UserRole)> GetBoardWithUserRoleAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default)
    {
        var result = await _dbSet
            .Where(b => b.Id == boardId)
            .Select(b => new
            {
                Board = b,
                UserRole = b.Members
                    .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
                    .Select(m => m.Role)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        return (result?.Board, result?.UserRole);
    }

    public async Task<BoardDetailsDto?> GetBoardDetailsByIdAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet
            .Where(b => b.Id == boardId)
            .Select(b => new BoardDetailsDto(
                b.Id,
                b.Name,
                b.CreatedAtUtc,
                b.Members
                    .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
                    .Select(m => m.Role)
                    .First(),
                b.Members
                    .OrderBy(m => m.JoinedAtUtc)
                    .Select(m => new UserDto(m.User!.Id, m.User.Email, m.User.DisplayName)),
                b.Columns
                    .OrderBy(c => c.Position)
                    .Select(c => new ColumnDto(
                        c.Id,
                        c.Name,
                        c.Position,
                        c.Tasks
                            .OrderBy(t => t.Position)
                            .Select(t => new BoardTaskPreviewDto(
                                t.Id,
                                t.Title,
                                t.Position))))))
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);

    public async Task<PagedList<BoardLookupDto>> GetBoardsByUserAzureAOIAsync(
        Guid azureAdObjectId,
        int pageNumber,
        int pageSize,
        TextSearchOptions<BoardSearchField>? searchOptions = null,
        CancellationToken ct = default)
    {
        var fields = SearchFieldsResolver.Resolve(searchOptions?.SearchIn, BoardSearchFields.Map);

        return await _dbSet
            .Where(b => b.Members.Any(m => m.User!.AzureAdObjectId == azureAdObjectId))
            .ApplyTextSearch(searchOptions?.Search, fields)
            .Select(b => new
            {
                Board = b,
                BoardActivity = b.LastModifiedAtUtc ?? b.CreatedAtUtc,

                ColumnsActivity = b.Columns
                    .Select(c => (DateTime?)(c.LastModifiedAtUtc ?? c.CreatedAtUtc))
                    .Max(),

                TasksActivity = b.Columns
                    .SelectMany(c => c.Tasks)
                    .Select(t => (DateTime?)(t.LastModifiedAtUtc ?? t.CreatedAtUtc))
                    .Max()
            })
            .Select(x => new
            {
                x.Board,
                x.TasksActivity,
                // Most recent activity of the board itself or any of its columns (null columns activity loses the comparison)
                BoardOrColumnsActivity = x.ColumnsActivity > x.BoardActivity ? x.ColumnsActivity.Value : x.BoardActivity
            })
            .Select(x => new
            {
                x.Board,
                // Fold task activity into the running max to get the overall most recent activity
                LastActivity = x.TasksActivity > x.BoardOrColumnsActivity ? x.TasksActivity.Value : x.BoardOrColumnsActivity
            })
            .OrderByDescending(x => x.LastActivity)
            .ToPagedAsync(
                x => new BoardLookupDto(
                    x.Board.Id,
                    x.Board.Name,
                    x.Board.CreatedAtUtc,
                    x.Board.Members
                        .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
                        .Select(m => m.Role)
                        .First(),
                    x.Board.Members
                        .Where(m => m.Role == BoardMemberRole.Owner)
                        .Select(m => new UserDto(m.User!.Id, m.User.Email, m.User.DisplayName))
                        .FirstOrDefault()),
                pageNumber,
                pageSize,
                ct);
    }

    public async Task<bool> IsBoardMemberAsync(Guid boardId, Guid userId, CancellationToken ct = default) =>
        await _context.BoardMembers.AnyAsync(m => m.BoardId == boardId && m.UserId == userId, ct);
}