using Azure.Storage.Blobs;
using JustTaskTracker.Infrastructure.Common.Constants;
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

        return services;
    }
}
