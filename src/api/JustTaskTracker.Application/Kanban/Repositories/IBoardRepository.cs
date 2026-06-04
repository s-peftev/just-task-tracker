using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Kanban.DTOs;
using JustTaskTracker.Domain.Kanban.Entities;
using JustTaskTracker.Domain.Kanban.Enums;

namespace JustTaskTracker.Application.Kanban.Repositories;

public interface IBoardRepository : IRepository<Board, Guid>
{
    void AddMember(BoardMember member);

    Task<(Board? Board, BoardMemberRole? UserRole)> GetBoardWithUserRoleAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardAccessStatus> GetBoardAccessAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardDetailsDto?> GetBoardDetailsByIdAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<PagedList<BoardLookupDto>> GetBoardsByUserAzureAOIAsync(Guid azureAdObjectId, int pageNumber, int pageSize, CancellationToken ct = default);
}