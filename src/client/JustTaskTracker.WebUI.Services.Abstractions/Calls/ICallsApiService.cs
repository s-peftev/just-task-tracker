using JustTaskTracker.WebUI.Domain.Calls;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

public interface ICallsApiService
{
    Task<IReadOnlyList<CallSessionDto>> GetActiveCallsAsync(Guid boardId, CancellationToken ct = default);

    Task<CallSessionDto> CreateCallAsync(CreateCallRequest request, CancellationToken ct = default);

    Task<JoinCallResponse> JoinCallAsync(Guid callSessionId, CancellationToken ct = default);

    Task EndCallAsync(Guid callSessionId, CancellationToken ct = default);
}
