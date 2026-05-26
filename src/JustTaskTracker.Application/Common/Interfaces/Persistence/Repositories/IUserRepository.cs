using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Entities;

namespace JustTaskTracker.Application.Common.Interfaces.Persistence.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<UserWithRolesDto?> GetUserWithRolesDtoByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default);
    Task<User?> GetUserWithRolesByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default);
}
