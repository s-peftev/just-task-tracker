using JustTaskTracker.Infrastructure.DI.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
    {
        services
            .AddAzureModule(configuration)
            .AddOptionsModule(configuration)
            .AddCorsModule()
            .AddAuthenticationModule(configuration)
            .AddUtilsModule()
            .AddServicesModule()
            .AddHangfireModule(configuration)
            .AddBillingModule()
            .AddCallsModule(configuration);

        return services;
    }
}
