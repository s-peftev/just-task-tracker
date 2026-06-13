using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Entities;

namespace JustTaskTracker.Application.Auth.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<UserDto?> GetUserDtoByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default);

    Task<User?> GetUserByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default);
}
