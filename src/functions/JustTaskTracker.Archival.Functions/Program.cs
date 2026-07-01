using Azure.Monitor.OpenTelemetry.Exporter;
using JustTaskTracker.Archival.Functions.Abstractions;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.ExternalProviders.CosmosDB;
using JustTaskTracker.Archival.Functions.Processing;
using JustTaskTracker.Archival.Functions.Processing.Export;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services
    .AddSingleton<IBoardExportProcessor, BoardExportProcessor>()
    .AddSingleton<ExportContextResolver>()
    .AddSingleton<IBoardExportDocumentClient, CosmosBoardExportDocumentClient>()
    .AddSingleton<IBoardExportCompletionHandler, InitialExportCompletionHandler>()
    .AddSingleton<IBoardExportCompletionHandler, ReExportCompletionHandler>()
    .AddSingleton<BoardExportCompletionHandlerRegistry>();

builder.Build().Run();
