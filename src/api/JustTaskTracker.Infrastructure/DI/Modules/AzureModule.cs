using Azure.Storage.Blobs;
using JustTaskTracker.Application.Common.Interfaces.ExternalProviders;
using JustTaskTracker.Infrastructure.Common.Constants;
using JustTaskTracker.Infrastructure.Common.ExternalProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class AzureModule
{
    internal static IServiceCollection AddAzureModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAzureBlobStorage(configuration);

        return services;
    }

    private static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringNames.BlobStorage)
            ?? throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");

        services.AddSingleton(new BlobServiceClient(connectionString));
        services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }
}
