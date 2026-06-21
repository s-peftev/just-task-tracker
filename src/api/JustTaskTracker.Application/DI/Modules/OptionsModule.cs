using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Application.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Application.DI.Modules;

internal static class OptionsModule
{
    internal static IServiceCollection AddOptionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var validationSettingsOptions = configuration
            .GetSection(ConfigSections.ValidationSettings)
            .Get<ValidationSettings>() ?? new ValidationSettings();

        services.AddSingleton(validationSettingsOptions);

        var blobStorageSettings = configuration
            .GetSection(ConfigSections.BlobStorage)
            .Get<BlobStorageSettings>()
            ?? throw new InvalidOperationException("BlobStorageContainers section is not configured.");

        blobStorageSettings.Validate();
        services.AddSingleton(blobStorageSettings);

        return services;
    }
}
