using JustTaskTracker.WebUI.Domain.Common;
using JustTaskTracker.WebUI.Domain.Kanban;

namespace JustTaskTracker.WebUI.Services.Abstractions.Kanban;

public interface IBoardApiService
{
    Task<PagedList<BoardLookupDto>> GetMyBoardsAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<BoardDetailsDto> GetBoardByIdAsync(Guid boardId, CancellationToken ct = default);
    Task<BoardDetailsDto> CreateBoardAsync(string name, CancellationToken ct = default);
    Task<BoardDetailsDto> UpdateBoardAsync(Guid boardId, string name, CancellationToken ct = default);
    Task DeleteBoardAsync(Guid boardId, CancellationToken ct = default);
}
