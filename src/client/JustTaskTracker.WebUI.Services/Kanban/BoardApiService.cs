using JustTaskTracker.WebUI.Domain.Common;
using JustTaskTracker.WebUI.Domain.Kanban;
using JustTaskTracker.WebUI.Domain.Kanban.Requests;
using JustTaskTracker.WebUI.Services.Abstractions.Kanban;
using JustTaskTracker.WebUI.Services.Api;

namespace JustTaskTracker.WebUI.Services.Kanban;

internal class BoardApiService(IBoardApi api) : IBoardApiService
{
    public async Task<PagedList<BoardLookupDto>> GetMyBoardsAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var response = await api.GetMyAsync(pageNumber, pageSize, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<BoardDetailsDto> GetBoardByIdAsync(Guid boardId, CancellationToken ct = default)
    {
        var response = await api.GetByIdAsync(boardId, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<BoardDetailsDto> CreateBoardAsync(string name, CancellationToken ct = default)
    {
        var response = await api.CreateAsync(new SaveBoardRequest(name), ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<BoardDetailsDto> UpdateBoardAsync(Guid boardId, string name, CancellationToken ct = default)
    {
        var response = await api.UpdateAsync(boardId, new SaveBoardRequest(name), ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task DeleteBoardAsync(Guid boardId, CancellationToken ct = default)
    {
        var response = await api.DeleteAsync(boardId, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }
}
