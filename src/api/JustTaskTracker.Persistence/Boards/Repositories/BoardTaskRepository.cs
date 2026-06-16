using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Helpers;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs.Attachments;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Searching;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Extentions;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class BoardTaskRepository(JustTaskTrackerDbContext context)
    : Repository<BoardTask, Guid>(context), IBoardTaskRepository
{
    public async Task<(BoardTask? BoardTask, BoardMemberRole? UserRole)> GetBoardTaskWithUserRoleAsync(Guid boardTaskId, Guid azureAdObjectId, CancellationToken ct = default)
    {
        var result = await _dbSet
            .Where(t => t.Id == boardTaskId)
            .Select(t => new
            {
                BoardTask = t,
                UserRole = t.Column!.Board!.Members
                    .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
                    .Select(m => m.Role)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        return (result?.BoardTask, result?.UserRole);
    }

    public async Task<BoardMemberRole?> GetUserRoleAsync(Guid boardTaskId, Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet
            .Where(task => task.Id == boardTaskId)
            .SelectMany(task => task.Column!.Board!.Members)
            .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(ct);

    public async Task<BoardTaskDetailsDto?> GetBoardTaskDetailsAsync(Guid boardTaskId, CancellationToken ct = default) =>
        await _dbSet
            .Where(task => task.Id == boardTaskId)
            .Select(t => new BoardTaskDetailsDto(
                t.Id,
                t.ColumnId,
                t.Column!.Name,
                t.Title,
                t.Position,
                t.CreatedAtUtc,
                new UserDto(t.Reporter!.Id, t.Reporter.Email, t.Reporter.DisplayName),
                default,
                t.Attachments
                    .OrderBy(attachment => attachment.Position)
                    .Select(a => new BoardTaskAttachmentDto(
                        a.Id,
                        a.OriginalFileName,
                        a.ContentType,
                        a.FileSizeBytes,
                        a.Position,
                        a.CreatedAtUtc,
                        new UserDto(
                            a.UploadedBy!.Id,
                            a.UploadedBy.Email,
                            a.UploadedBy.DisplayName)))
                    .ToList(),
                t.Description,
                t.Assignee == null
                    ? null
                    : new UserDto(t.Assignee.Id, t.Assignee.Email, t.Assignee.DisplayName)))
            .FirstOrDefaultAsync(ct);

    public async Task<PagedList<BoardTaskLookupDto>> GetBoardTaskLookupListAsync(
        int pageNumber,
        int pageSize,
        TextSearchOptions<BoardTaskSearchField>? searchOptions = null,
        CancellationToken ct = default)
    {
        var fields = SearchFieldsResolver.Resolve(searchOptions?.SearchIn, BoardTaskSearchFields.Map);

        return await _dbSet
            .ApplyTextSearch(searchOptions?.Search, fields)
            .OrderByDescending(t => t.LastModifiedAtUtc)
            .ThenByDescending(t => t.CreatedAtUtc)
            .ToPagedAsync(
                t => new BoardTaskLookupDto(
                    t.Id,
                    t.Title,
                    t.Description),
                pageNumber,
                pageSize,
                ct);
    }

    public async Task<IReadOnlyList<BoardTask>> GetListByColumnIdAsync(Guid columnId, CancellationToken ct = default) =>
        await _dbSet
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Position)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BoardTask>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(t => t.Column!.BoardId == boardId)
            .OrderBy(t => t.Position)
            .ToListAsync(ct);

    public async Task<int> GetCountByColumnIdAsync(Guid columnId, CancellationToken ct = default) =>
        await _dbSet.CountAsync(t => t.ColumnId == columnId, ct);

    public void RemoveRange(IReadOnlyList<BoardTask> tasks)
    {
        foreach (var task in tasks)
            _dbSet.Remove(task);
    }

    public async Task<(BoardTaskAttachment? Attachment, BoardMemberRole? UserRole)> GetAttachmentWithUserRoleAsync(Guid attachmentId, Guid azureAdObjectId, CancellationToken ct = default)
    {
        var result = await _context.Set<BoardTaskAttachment>()
            .Where(attachment => attachment.Id == attachmentId)
            .Select(attachment => new
            {
                Attachment = attachment,
                UserRole = attachment.BoardTask!.Column!.Board!.Members
                    .Where(m => m.User!.AzureAdObjectId == azureAdObjectId)
                    .Select(m => m.Role)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        return (result?.Attachment, result?.UserRole);
    }

    public async Task<int> GetAttachmentsCountAsync(Guid boardTaskId, CancellationToken ct = default) =>
        await _context.Set<BoardTaskAttachment>()
            .CountAsync(a => a.BoardTaskId == boardTaskId, ct);

    public async Task<IReadOnlyList<BoardTaskAttachment>> GetAttachmentsAsync(Guid boardTaskId, CancellationToken ct = default) =>
        await _context.Set<BoardTaskAttachment>()
            .Where(attachment => attachment.BoardTaskId == boardTaskId)
            .OrderBy(attachment => attachment.Position)
            .ToListAsync(ct);

    public void RemoveAttachment(BoardTaskAttachment attachment) =>
        _context.Set<BoardTaskAttachment>().Remove(attachment);
}
