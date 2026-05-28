using JustTaskTracker.Domain.Auth.Entities;

namespace JustTaskTracker.Application.Common.Interfaces.Persistence.Repositories;

public interface IUserRoleRepository
{
    UserRole Add(UserRole userRole);
    void Remove(UserRole userRole);
}
