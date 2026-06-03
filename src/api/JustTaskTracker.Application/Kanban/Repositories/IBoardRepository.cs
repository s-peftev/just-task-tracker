using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Kanban.DTOs;
using JustTaskTracker.Domain.Kanban.Entities;
using JustTaskTracker.Domain.Kanban.Enums;

namespace JustTaskTracker.Application.Kanban.Repositories;

public interface IBoardRepository : IRepository<Board, Guid>
{
    Task<BoardAccessStatus> GetBoardAccessAsync(Guid boardId, Guid azureAdObjectId, CancellationToken ct = default);

    Task<BoardDetailsDto?> GetBoardDetailsByIdAsync(Guid boardId, CancellationToken ct = default);

    Task<PagedList<BoardLookupDto>> GetBoardsByUserAzureAOIAsync(Guid azureAdObjectId, int pageNumber, int pageSize, CancellationToken ct = default);
}