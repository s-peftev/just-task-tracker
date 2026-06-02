using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Auth.Repositories;

public class UserRepository(JustTaskTrackerDbContext context) : Repository<User, Guid>(context), IUserRepository
{
    public async Task<UserDto?> GetUserDtoByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet
            .Where(u => u.AzureAdObjectId == azureAdObjectId)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.DisplayName
            ))
            .FirstOrDefaultAsync(ct);

    public async Task<User?> GetUserByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(u => u.AzureAdObjectId == azureAdObjectId, ct);
}
