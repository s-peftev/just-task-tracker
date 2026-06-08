using JustTaskTracker.WebUI.Domain.Common;
using JustTaskTracker.WebUI.Domain.Boards;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

public interface IBoardApiService
{
    Task<PagedList<BoardLookupDto>> GetMyBoardsAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<BoardDetailsDto> GetBoardByIdAsync(Guid boardId, CancellationToken ct = default);
    Task<BoardDetailsDto> CreateBoardAsync(string name, CancellationToken ct = default);
    Task<BoardDetailsDto> UpdateBoardAsync(Guid boardId, string name, CancellationToken ct = default);
    Task DeleteBoardAsync(Guid boardId, CancellationToken ct = default);
}
