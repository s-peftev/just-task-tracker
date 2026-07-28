using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallParticipantRepository
{
    Task<CallParticipant?> GetActiveParticipantAsync(Guid callSessionId, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<CallParticipant>> GetActiveParticipantsAsync(Guid callSessionId, CancellationToken ct = default);

    void Add(CallParticipant participant);
}
