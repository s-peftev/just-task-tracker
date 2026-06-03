using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Kanban.DTOs;
using JustTaskTracker.Domain.Kanban.Entities;

namespace JustTaskTracker.Application.Kanban.Repositories;

public interface IBoardRepository : IRepository<Board, Guid>
{ 
    Task<PagedList<BoardLookupDto>> GetBoardsByUserAzureAOIAsync(Guid azureAdObjectId, int pageNumber, int pageSize, CancellationToken ct = default);
}
