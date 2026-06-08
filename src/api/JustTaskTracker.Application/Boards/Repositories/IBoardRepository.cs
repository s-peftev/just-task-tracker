using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Repositories;

public interface IBoardRepository : IRepository<Board, Guid>
{
    void AddMember(BoardMember member);

    Task<(Board? Board, BoardMemberRole? UserRole)> GetBoardWithUserRoleAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardAccessStatus> GetBoardAccessAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardDetailsDto?> GetBoardDetailsByIdAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<PagedList<BoardLookupDto>> GetBoardsByUserAzureAOIAsync(Guid azureAdObjectId, int pageNumber, int pageSize, CancellationToken ct = default);
}