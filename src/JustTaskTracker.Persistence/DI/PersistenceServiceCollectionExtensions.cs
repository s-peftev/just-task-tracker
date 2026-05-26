using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Common.Interfaces.Persistence.Repositories;
using JustTaskTracker.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Persistence.DI;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
        AddScoped(services);

        return services;
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("JustTaskTracker")
            ?? throw new InvalidOperationException("DB connection string is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
    }

    private static void AddScoped(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
    }
}
