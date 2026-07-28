using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallSessionAllowedParticipantRepository
{
    Task<bool> IsAllowedAsync(Guid callSessionId, Guid userId, CancellationToken ct = default);

    void Add(CallSessionAllowedParticipant allowedParticipant);
}
