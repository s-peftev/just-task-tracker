using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Search.Documents;
using Azure.Storage.Blobs;
using JustTaskTracker.Application.Assistant.Abstractions;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Infrastructure.Assistant;
using JustTaskTracker.Infrastructure.Boards.Export;
using JustTaskTracker.Infrastructure.Common.Constants;
using JustTaskTracker.Infrastructure.Common.ExternalProviders;
using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using System.ClientModel;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class AzureModule
{
    internal static IServiceCollection AddAzureModule(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAzureBlobStorage(configuration)
            .AddAzureServiceBus(configuration)
            .AddAzureCosmosDb(configuration)
            .AddAzureSignalR(configuration)
            .AddAzureAiSearch()
            .AddAzureOpenAi();

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
            var sbOptions = sp.GetRequiredService<ServiceBusOptions>();

            return client.CreateSender(sbOptions.QueueNames!.BoardArchivingQueueName);
        });

        services.AddSingleton<IBoardExportQueueSender, AzureBoardExportQueueSender>();

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

    private static IServiceCollection AddAzureSignalR(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringNames.SignalR)
            ?? throw new InvalidOperationException("SignalR connection string is not configured.");

        services.AddSignalR().AddAzureSignalR(options =>
        {
            options.ConnectionString = connectionString;
        });

        return services;
    }

    private static IServiceCollection AddAzureAiSearch(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<AiSearchOptions>();

            return new SearchClient(
                new Uri(options.Endpoint),
                options.IndexName,
                new AzureKeyCredential(options.ApiKey));
        });

        services.AddSingleton<IKnowledgeBaseSearchService, AzureAiSearchKnowledgeService>();

        return services;
    }

    private static IServiceCollection AddAzureOpenAi(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<AzureOpenAiOptions>();

            var client = new OpenAIClient(
                new ApiKeyCredential(options.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(options.Endpoint)
                });

            return client.GetChatClient(options.ChatDeploymentName);
        });

        services.AddSingleton<IAssistantCompletionService, AzureOpenAiCompletionService>();

        return services;
    }
}
