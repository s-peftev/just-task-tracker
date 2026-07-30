using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Calls;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

public interface ICallsApiService
{
    Task<IReadOnlyList<CallSessionDto>> GetActiveCallsAsync(Guid boardId, CancellationToken ct = default);

    Task<PagedList<CallSessionHistoryDto>> GetHistoryAsync(Guid boardId, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<CallSessionDto> CreateCallAsync(CreateCallRequest request, CancellationToken ct = default);

    Task<JoinCallResponse> JoinCallAsync(Guid callSessionId, CancellationToken ct = default);

    Task<IReadOnlyList<CallParticipantDto>> GetParticipantsAsync(Guid callSessionId, CancellationToken ct = default);

    Task<IReadOnlyList<BoardTaskDetailsDto>> GetLinkedTasksAsync(Guid callSessionId, CancellationToken ct = default);

    Task EndCallAsync(Guid callSessionId, CancellationToken ct = default);

    Task RequestScreenShareAsync(Guid callSessionId, CancellationToken ct = default);

    Task StopScreenShareAsync(Guid callSessionId, CancellationToken ct = default);
}
