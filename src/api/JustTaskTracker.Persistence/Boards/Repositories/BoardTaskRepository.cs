using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class BoardTaskRepository(JustTaskTrackerDbContext context)
    : Repository<BoardTask, Guid>(context), IBoardTaskRepository
{
    public async Task<(BoardTask? BoardTask, BoardMemberRole? UserRole)> GetBoardTaskWithUserRoleAsync(Guid boardTaskId, Guid azureAdObjectId, CancellationToken ct = default)
    {
        var result = await _dbSet
            .Where(task => task.Id == boardTaskId)
            .Select(task => new
            {
                BoardTask = task,
                UserRole = task.Column!.Board!.Members
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
            .Select(task => new BoardTaskDetailsDto(
                task.Id,
                task.ColumnId,
                task.Column!.Name,
                task.Title,
                task.Position,
                task.CreatedAtUtc,
                new UserDto(task.Reporter!.Id, task.Reporter.Email, task.Reporter.DisplayName),
                default,
                task.Attachments
                    .OrderBy(attachment => attachment.Position)
                    .Select(attachment => new BoardTaskAttachmentDto(
                        attachment.Id,
                        attachment.OriginalFileName,
                        attachment.ContentType,
                        attachment.FileSizeBytes,
                        attachment.Position,
                        attachment.CreatedAtUtc,
                        new UserDto(
                            attachment.UploadedBy!.Id,
                            attachment.UploadedBy.Email,
                            attachment.UploadedBy.DisplayName)))
                    .ToList(),
                task.Description,
                task.Assignee == null
                    ? null
                    : new UserDto(task.Assignee.Id, task.Assignee.Email, task.Assignee.DisplayName)))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<BoardTask>> GetListByColumnIdAsync(Guid columnId, CancellationToken ct = default) =>
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
