using JustTaskTracker.Infrastructure.DI.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAppOptionsModule(configuration)
            .AddCorsModule()
            .AddAuthenticationModule(configuration)
            .AddUtilsModule()
            .AddAzureModule(configuration);

        return services;
    }
}
