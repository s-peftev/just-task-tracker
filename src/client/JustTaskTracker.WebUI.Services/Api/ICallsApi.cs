using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Calls;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface ICallsApi
{
    [Get("/api/calls")]
    Task<IApiResponse<ApiEnvelope<List<CallSessionDto>>>> GetActiveAsync(Guid boardId, CancellationToken ct = default);

    [Get("/api/calls/history")]
    Task<IApiResponse<ApiEnvelope<PagedList<CallSessionHistoryDto>>>> GetHistoryAsync(
        Guid boardId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    [Post("/api/calls")]
    Task<IApiResponse<ApiEnvelope<CallSessionDto>>> CreateAsync([Body] CreateCallRequest request, CancellationToken ct = default);

    [Post("/api/calls/{id}/join")]
    Task<IApiResponse<ApiEnvelope<JoinCallResponse>>> JoinAsync(Guid id, CancellationToken ct = default);

    [Get("/api/calls/{id}/participants")]
    Task<IApiResponse<ApiEnvelope<List<CallParticipantDto>>>> GetParticipantsAsync(Guid id, CancellationToken ct = default);

    [Get("/api/calls/{id}/linked-tasks")]
    Task<IApiResponse<ApiEnvelope<List<BoardTaskDetailsDto>>>> GetLinkedTasksAsync(Guid id, CancellationToken ct = default);

    [Post("/api/calls/{id}/end")]
    Task<IApiResponse<ApiEnvelope<object>>> EndAsync(Guid id, CancellationToken ct = default);
}
