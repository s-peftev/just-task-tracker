using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Common.Interfaces.Persistence.Repositories;
using JustTaskTracker.Persistence.Auth.Repositories;
using JustTaskTracker.Persistence.Common;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Persistence.DI.Modules;

internal static class RepositoriesModule
{
    public static IServiceCollection AddRepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        return services;
    }
}
