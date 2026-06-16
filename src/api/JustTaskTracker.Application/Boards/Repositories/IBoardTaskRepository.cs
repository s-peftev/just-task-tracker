using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IBoardTaskRepository : IRepository<BoardTask, Guid>
{
    Task<(BoardTask? BoardTask, BoardMemberRole? UserRole)> GetBoardTaskWithUserRoleAsync(Guid boardTaskId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardMemberRole?> GetUserRoleAsync(Guid boardTaskId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardTaskDetailsDto?> GetBoardTaskDetailsAsync(Guid boardTaskId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTask>> GetListByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTask>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default);

    Task<int> GetCountByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    void RemoveRange(IReadOnlyList<BoardTask> tasks);

    Task<(BoardTaskAttachment? Attachment, BoardMemberRole? UserRole)> GetAttachmentWithUserRoleAsync(Guid attachmentId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<int> GetAttachmentsCountAsync(Guid boardTaskId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTaskAttachment>> GetAttachmentsAsync(Guid boardTaskId, CancellationToken ct = default);

    void RemoveAttachment(BoardTaskAttachment attachment);
}
