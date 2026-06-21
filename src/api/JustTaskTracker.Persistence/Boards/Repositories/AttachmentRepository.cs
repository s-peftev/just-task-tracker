using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class AttachmentRepository(JustTaskTrackerDbContext context)
    : Repository<BoardTaskAttachment, Guid>(context), IAttachmentRepository
{
    public async Task<(BoardTaskAttachment? Attachment, BoardMemberRole? UserRole)> GetAttachmentWithUserRoleAsync(Guid attachmentId, Guid azureAdObjectId, CancellationToken ct = default)
    {
        var result = await _dbSet
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

    public async Task<int> GetCountByBoardTaskIdAsync(Guid boardTaskId, CancellationToken ct = default) =>
        await _dbSet.CountAsync(a => a.BoardTaskId == boardTaskId, ct);

    public async Task<IReadOnlyList<BoardTaskAttachment>> GetListByBoardTaskIdAsync(Guid boardTaskId, CancellationToken ct = default) =>
        await _dbSet
            .Where(attachment => attachment.BoardTaskId == boardTaskId)
            .OrderBy(attachment => attachment.Position)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BoardTaskAttachment>> GetListByColumnIdAsync(Guid columnId, CancellationToken ct = default) =>
        await _dbSet
            .Where(attachment => attachment.BoardTask!.ColumnId == columnId)
            .OrderBy(attachment => attachment.BoardTaskId)
            .ThenBy(attachment => attachment.Position)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BoardTaskAttachment>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default) =>
        await _dbSet
            .Where(attachment => attachment.BoardTask!.Column!.BoardId == boardId)
            .OrderBy(attachment => attachment.BoardTaskId)
            .ThenBy(attachment => attachment.Position)
            .ToListAsync(ct);
}
