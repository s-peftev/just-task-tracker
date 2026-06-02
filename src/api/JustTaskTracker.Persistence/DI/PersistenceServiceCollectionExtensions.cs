using JustTaskTracker.Persistence.DI.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Persistence.DI;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabaseModule(configuration)
            .AddRepositoriesModule();

        return services;
    }
}
