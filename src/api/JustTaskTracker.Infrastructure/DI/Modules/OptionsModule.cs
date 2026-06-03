using JustTaskTracker.Infrastructure.Common.Constants;
using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class OptionsModule
{
    internal static IServiceCollection AddAppOptionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendOptions = configuration
            .GetSection(ConfigSections.Frontend)
            .Get<FrontendOptions>() ?? new FrontendOptions();

        services.AddSingleton(frontendOptions);

        var azureAdOptions = configuration
            .GetSection(ConfigSections.AzureAd)
            .Get<AzureAdOptions>() ?? new AzureAdOptions();

        services.AddSingleton(azureAdOptions);

        var paginationDefaultsOptions = configuration
            .GetSection(ConfigSections.PaginationDefaults)
            .Get<PaginationDefaultsOptions>() ?? new PaginationDefaultsOptions();

        services.AddSingleton(paginationDefaultsOptions);

        return services;
    }
}
