using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Calls;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Services.Abstractions.Calls;
using JustTaskTracker.WebUI.Services.Api;

namespace JustTaskTracker.WebUI.Services.Calls;

internal class CallsApiService(ICallsApi api) : ICallsApiService
{
    public async Task<IReadOnlyList<CallSessionDto>> GetActiveCallsAsync(Guid boardId, CancellationToken ct = default)
    {
        var response = await api.GetActiveAsync(boardId, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<PagedList<CallSessionHistoryDto>> GetHistoryAsync(Guid boardId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var response = await api.GetHistoryAsync(boardId, pageNumber, pageSize, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<CallSessionDto> CreateCallAsync(CreateCallRequest request, CancellationToken ct = default)
    {
        var response = await api.CreateAsync(request, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<JoinCallResponse> JoinCallAsync(Guid callSessionId, CancellationToken ct = default)
    {
        var response = await api.JoinAsync(callSessionId, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<IReadOnlyList<CallParticipantDto>> GetParticipantsAsync(Guid callSessionId, CancellationToken ct = default)
    {
        var response = await api.GetParticipantsAsync(callSessionId, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<IReadOnlyList<BoardTaskDetailsDto>> GetLinkedTasksAsync(Guid callSessionId, CancellationToken ct = default)
    {
        var response = await api.GetLinkedTasksAsync(callSessionId, ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task EndCallAsync(Guid callSessionId, CancellationToken ct = default)
    {
        var response = await api.EndAsync(callSessionId, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task RequestScreenShareAsync(Guid callSessionId, CancellationToken ct = default)
    {
        var response = await api.StartScreenShareAsync(callSessionId, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }

    public async Task StopScreenShareAsync(Guid callSessionId, CancellationToken ct = default)
    {
        var response = await api.StopScreenShareAsync(callSessionId, ct);

        ApiResponseGuard.EnsureSuccess(response);
    }
}
