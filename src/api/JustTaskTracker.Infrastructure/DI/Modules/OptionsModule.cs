using JustTaskTracker.Infrastructure.Common.Constants;
using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class OptionsModule
{
    internal static IServiceCollection AddOptionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendOptions = configuration
            .GetSection(ConfigSections.Frontend)
            .Get<FrontendOptions>() ?? new FrontendOptions();

        services.AddSingleton(frontendOptions);

        var azureAdOptions = configuration
            .GetSection(ConfigSections.AzureAd)
            .Get<AzureAdOptions>()
            ?? throw new InvalidOperationException($"{ConfigSections.AzureAd} section is not configured.");

        azureAdOptions.Validate();
        services.AddSingleton(azureAdOptions);

        var paginationDefaultsOptions = configuration
            .GetSection(ConfigSections.PaginationDefaults)
            .Get<PaginationDefaultsOptions>()
            ?? throw new InvalidOperationException($"{ConfigSections.PaginationDefaults} section is not configured.");

        paginationDefaultsOptions.Validate();
        services.AddSingleton(paginationDefaultsOptions);

        var serviceBusOptions = configuration
            .GetSection(ConfigSections.ServiceBus)
            .Get<ServiceBusOptions>()
            ?? throw new InvalidOperationException($"{ConfigSections.ServiceBus} section is not configured.");

        serviceBusOptions.Validate();
        services.AddSingleton(serviceBusOptions);

        var cosmosDbOptions = configuration
            .GetSection(ConfigSections.CosmosDB)
            .Get<CosmosDbOptions>()
            ?? throw new InvalidOperationException($"{ConfigSections.CosmosDB} section is not configured.");

        cosmosDbOptions.Validate();
        services.AddSingleton(cosmosDbOptions);

        return services;
    }
}
