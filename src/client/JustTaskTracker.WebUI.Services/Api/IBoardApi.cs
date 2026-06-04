using JustTaskTracker.WebUI.Domain.Common;
using JustTaskTracker.WebUI.Domain.Kanban;
using JustTaskTracker.WebUI.Domain.Kanban.Requests;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface IBoardApi
{
    [Get("/api/boards")]
    Task<IApiResponse<ApiEnvelope<PagedList<BoardLookupDto>>>> GetMyAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    [Get("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<BoardDetailsDto>>> GetByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/boards")]
    Task<IApiResponse<ApiEnvelope<BoardDetailsDto>>> CreateAsync([Body] SaveBoardRequest request, CancellationToken ct = default);

    [Put("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<BoardDetailsDto>>> UpdateAsync(Guid id, [Body] SaveBoardRequest request, CancellationToken ct = default);

    [Delete("/api/boards/{id}")]
    Task<IApiResponse<ApiEnvelope<object>>> DeleteAsync(Guid id, CancellationToken ct = default);
}
