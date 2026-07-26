using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface IAcsUserIdentityMappingRepository
{
    Task<AcsUserIdentityMapping?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<AcsUserIdentityMapping?> GetByAcsCommunicationUserIdAsync(string acsCommunicationUserId, CancellationToken ct = default);

    void Add(AcsUserIdentityMapping mapping);
}
