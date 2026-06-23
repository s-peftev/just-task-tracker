using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Application.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Application.DI.Modules;

internal static class OptionsModule
{
    internal static IServiceCollection AddOptionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var validationSettings = configuration
            .GetSection(ConfigSections.ValidationSettings)
            .Get<ValidationSettings>()
            ?? throw new InvalidOperationException($"{ConfigSections.ValidationSettings} section is not configured.");

        validationSettings.Validate();
        services.AddSingleton(validationSettings);

        var blobStorageSettings = configuration
            .GetSection(ConfigSections.BlobStorage)
            .Get<BlobStorageSettings>()
            ?? throw new InvalidOperationException("BlobStorageContainers section is not configured.");

        blobStorageSettings.Validate();
        services.AddSingleton(blobStorageSettings);

        var profilePhotoProcessingSettings = configuration
            .GetSection(ConfigSections.ProfilePhotoProcessing)
            .Get<ProfilePhotoProcessingSettings>()
            ?? throw new InvalidOperationException($"{ConfigSections.ProfilePhotoProcessing} section is not configured.");

        profilePhotoProcessingSettings.Validate();
        services.AddSingleton(profilePhotoProcessingSettings);

        return services;
    }
}
