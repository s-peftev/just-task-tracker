using JustTaskTracker.Infrastructure.DI.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCorsModule(configuration)
            .AddAuthenticationModule(configuration);

        return services;
    }
}
