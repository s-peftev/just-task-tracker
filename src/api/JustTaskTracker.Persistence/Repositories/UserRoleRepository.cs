using JustTaskTracker.Application.Common.Interfaces.Persistence.Repositories;
using JustTaskTracker.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Repositories;

public class UserRoleRepository(AppDbContext context) : IUserRoleRepository
{
    private readonly DbSet<UserRole> _dbSet = context.Set<UserRole>();
    public UserRole Add(UserRole userRole)
    {
        _dbSet.Add(userRole);

        return userRole;
    }

    public void Remove(UserRole userRole)
    {
        _dbSet.Remove(userRole);
    }
}
