using Azure.Monitor.OpenTelemetry.Exporter;
using JustTaskTracker.Archival.Functions.Abstractions.Archiving;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Abstractions.Processing;
using JustTaskTracker.Archival.Functions.Archiving;
using JustTaskTracker.Archival.Functions.Archiving.Summary;
using JustTaskTracker.Archival.Functions.ExternalProviders.Api;
using JustTaskTracker.Archival.Functions.ExternalProviders.CosmosDB;
using JustTaskTracker.Archival.Functions.ExternalProviders.Http;
using JustTaskTracker.Archival.Functions.Processing;
using JustTaskTracker.Archival.Functions.Processing.Export;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services
    .Configure<BoardExportApiClientOptions>(
        builder.Configuration.GetSection(BoardExportApiClientOptions.SectionName))

    .AddHttpClient<IBoardExportDataApiClient, BoardExportDataApiClient>((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<BoardExportApiClientOptions>>().Value;
        options.Validate();

        client.BaseAddress = new Uri(options.BaseAddress.TrimEnd('/') + '/', UriKind.Absolute);
        client.Timeout = TimeSpan.FromMinutes(options.RequestTimeoutMinutes);
    })

    .Services
    .AddHttpClient<IExportAttachmentFetcher, HttpExportAttachmentFetcher>()
    .Services
    .AddSingleton<IBoardExportSummaryWriter, JsonBoardExportSummaryWriter>()
    .AddSingleton<BoardExportSummaryWriterRegistry>()
    .AddSingleton<IBoardArchiveBuilder, BoardArchiveBuilder>()
    .AddSingleton<IBoardExportProcessor, BoardExportProcessor>()
    .AddSingleton<ExportContextResolver>()
    .AddSingleton<IBoardExportDocumentClient, CosmosBoardExportDocumentClient>()
    .AddSingleton<IBoardExportCompletionHandler, InitialExportCompletionHandler>()
    .AddSingleton<IBoardExportCompletionHandler, ReExportCompletionHandler>()
    .AddSingleton<BoardExportCompletionHandlerRegistry>();

builder.Build().Run();
