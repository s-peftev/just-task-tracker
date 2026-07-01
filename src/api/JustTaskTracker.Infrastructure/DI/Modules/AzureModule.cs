using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Infrastructure.Common.Constants;
using JustTaskTracker.Infrastructure.Common.ExternalProviders;
using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JustTaskTracker.Infrastructure.Boards.Export;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class AzureModule
{
    internal static IServiceCollection AddAzureModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAzureBlobStorage(configuration);
        services.AddAzureServiceBus(configuration);
        services.AddAzureCosmosDb(configuration);

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

    private static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        var sbConnectionString = configuration.GetConnectionString(ConnectionStringNames.ServiceBus)
            ?? throw new InvalidOperationException("ServiceBus connection string is not configured.");

        services.AddSingleton(sp => new ServiceBusClient(sbConnectionString));

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            var options = sp.GetRequiredService<ServiceBusOptions>();

            return client.CreateSender(options.QueueNames!.BoardArchivingQueueName);
        });

        return services;
    }

    private static IServiceCollection AddAzureCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringNames.CosmosDB)
            ?? throw new InvalidOperationException("CosmosDB connection string is not configured.");

        services.AddSingleton(_ => new CosmosClient(connectionString));

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var options = sp.GetRequiredService<CosmosDbOptions>();

            return client.GetContainer(
                options.DatabaseName,
                options.Containers!.BoardExport);
        });

        services.AddSingleton<IBoardExportService, CosmosBoardExportService>();

        return services;
    }
}
