using JustTaskTracker.Application.Calls.ReadModels;
using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallParticipantRepository
{
    Task<CallParticipant?> GetActiveParticipantAsync(Guid callSessionId, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<CallParticipant>> GetActiveParticipantsAsync(Guid callSessionId, CancellationToken ct = default);

    Task<IReadOnlyList<CallParticipantHistoryReadModel>> GetParticipantHistoryAsync(Guid callSessionId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>> GetActiveParticipantUserIdsForSessionsAsync(IReadOnlyList<Guid> callSessionIds, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, IReadOnlyList<CallParticipantHistoryReadModel>>> GetParticipantHistoryForSessionsAsync(IReadOnlyList<Guid> callSessionIds, CancellationToken ct = default);

    void Add(CallParticipant participant);
}
