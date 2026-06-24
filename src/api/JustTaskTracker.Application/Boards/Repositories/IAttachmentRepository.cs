using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IAttachmentRepository : IRepository<BoardTaskAttachment, Guid>
{
    Task<(BoardTaskAttachment? Attachment, BoardMemberRole? UserRole)> GetAttachmentWithUserRoleAsync(Guid attachmentId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<int> GetCountByBoardTaskIdAsync(Guid boardTaskId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTaskAttachment>> GetListByBoardTaskIdAsync(Guid boardTaskId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTaskAttachment>> GetListByColumnIdAsync(Guid columnId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTaskAttachment>> GetListByBoardIdAsync(Guid boardId, CancellationToken ct = default);
}
