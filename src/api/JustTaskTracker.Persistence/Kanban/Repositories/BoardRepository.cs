using JustTaskTracker.Application.Kanban.Repositories;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Kanban.DTOs;
using JustTaskTracker.Domain.Kanban.Entities;
using JustTaskTracker.Domain.Kanban.Enums;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Extentions;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Kanban.Repositories;

public class BoardRepository(JustTaskTrackerDbContext context)
    : Repository<Board, Guid>(context), IBoardRepository
{
    public void AddMember(BoardMember member) =>
        _context.BoardMembers.Add(member);

    public async Task<BoardAccessStatus> GetBoardAccessAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default)
    {
        var access = await _dbSet
            .Where(b => b.Id == boardId)
            .Select(b => new
            {
                IsMember = b.Members.Any(m => m.User.AzureAdObjectId == azureAdObjectId)
            })
            .FirstOrDefaultAsync(ct);

        if (access is null)
            return BoardAccessStatus.NotFound;

        return access.IsMember
            ? BoardAccessStatus.Allowed
            : BoardAccessStatus.Forbidden;
    }

    public async Task<BoardDetailsDto?> GetBoardDetailsByIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(b => b.Id == boardId)
            .Select(b => new BoardDetailsDto(
                b.Id,
                b.Name,
                b.CreatedAtUtc,
                b.Members
                    .OrderBy(m => m.JoinedAtUtc)
                    .Select(m => new UserDto(m.User.Id, m.User.Email, m.User.DisplayName)),
                b.Columns
                    .OrderBy(c => c.Position)
                    .Select(c => new ColumnDto(
                        c.Id,
                        c.Name,
                        c.Position,
                        c.Tasks
                            .OrderBy(t => t.Position)
                            .Select(t => new TaskDto(
                                t.Id,
                                t.Title,
                                t.Description,
                                t.Position,
                                t.CreatedAtUtc,
                                t.AssigneeId == null
                                    ? null
                                    : new UserDto(t.Assignee!.Id, t.Assignee.Email, t.Assignee.DisplayName),
                                new UserDto(t.Reporter.Id, t.Reporter.Email, t.Reporter.DisplayName)))))))
            .FirstOrDefaultAsync(ct);

    public async Task<PagedList<BoardLookupDto>> GetBoardsByUserAzureAOIAsync(Guid azureAdObjectId, int pageNumber, int pageSize, CancellationToken ct = default) =>
        await _dbSet
            .Where(b => b.Members.Any(m => m.User.AzureAdObjectId == azureAdObjectId))
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
                        .Where(m => m.Role == BoardMemberRole.Owner)
                        .Select(m => new UserDto(m.User.Id, m.User.Email, m.User.DisplayName))
                        .FirstOrDefault(),
                    x.Board.Members
                        .Where(m => m.User.AzureAdObjectId == azureAdObjectId)
                        .Select(m => m.Role)
                        .First()),
                pageNumber,
                pageSize,
                ct);
}