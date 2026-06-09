using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface IBoardApi
{
    [Get("/api/boards")]
    Task<IApiResponse<ApiEnvelope<PagedList<BoardLookupDto>>>> GetMyAsync(
        int pageNumber,
        int pageSize,
        [AliasAs("TextSearchOptions.Search")] string? textSearchOptionsSearch = null,
        CancellationToken ct = default);

    [Get("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<BoardDetailsDto>>> GetByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/boards")]
    Task<IApiResponse<ApiEnvelope<BoardDetailsDto>>> CreateAsync([Body] SaveBoardRequest request, CancellationToken ct = default);

    [Put("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<object>>> UpdateAsync(Guid id, [Body] SaveBoardRequest request, CancellationToken ct = default);

    [Delete("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<object>>> DeleteAsync(Guid id, CancellationToken ct = default);

    [Post("/api/boards/{boardId}/columns")]
    Task<IApiResponse<ApiEnvelope<ColumnDto>>> CreateColumnAsync(
        Guid boardId,
        [Body] SaveColumnRequest request,
        CancellationToken ct = default);

    [Put("/api/boards/{boardId}/columns/{columnId}")]
    Task<IApiResponse<ApiEnvelope<object>>> UpdateColumnAsync(
        Guid boardId,
        Guid columnId,
        [Body] SaveColumnRequest request,
        CancellationToken ct = default);
}
