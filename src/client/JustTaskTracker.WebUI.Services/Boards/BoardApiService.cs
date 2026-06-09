using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Api;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Boards;

internal class BoardApiService(IBoardApi api) : IBoardApiService
{
    public async Task<PagedList<BoardLookupDto>> GetMyBoardsAsync(GetBoardsForCurrentUserRequest request, CancellationToken ct = default)
    {
        var search = string.IsNullOrWhiteSpace(request.TextSearchOptions?.Search)
            ? null
            : request.TextSearchOptions.Search;

        var response = await api.GetMyAsync(
            request.PageNumber!.Value,
            request.PageSize!.Value,
            search,
            ct);

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

    public async Task<ColumnDto> CreateColumnAsync(Guid boardId, string name, CancellationToken ct = default)
    {
        var response = await api.CreateColumnAsync(boardId, new SaveColumnRequest(name), ct);

        return ApiResponseGuard.Unwrap(response);
    }
}
