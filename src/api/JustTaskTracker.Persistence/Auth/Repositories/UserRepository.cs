using JustTaskTracker.Application.Common.Interfaces.Persistence.Repositories;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Auth.Repositories;

public class UserRepository(JustTaskTrackerDbContext context) : Repository<User, Guid>(context), IUserRepository
{
    public async Task<UserWithRolesDto?> GetUserWithRolesDtoByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet
            .Where(u => u.AzureAdObjectId == azureAdObjectId)
            .Select(u => new UserWithRolesDto(
                u.Id,
                u.Email,
                u.DisplayName,
                u.UserRoles.Select(ur => ur.Role.Name).ToList()
            ))
            .FirstOrDefaultAsync(ct);

    public async Task<User?> GetUserWithRolesByAzureAOIAsync(Guid azureAdObjectId, CancellationToken ct = default) =>
        await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.AzureAdObjectId == azureAdObjectId, ct);
}
