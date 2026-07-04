using JustTaskTracker.Infrastructure.DI.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptionsModule(configuration)
            .AddCorsModule()
            .AddAuthenticationModule(configuration)
            .AddUtilsModule()
            .AddServicesModule()
            .AddAzureModule(configuration)
            .AddHangfireModule(configuration);

        return services;
    }
}
