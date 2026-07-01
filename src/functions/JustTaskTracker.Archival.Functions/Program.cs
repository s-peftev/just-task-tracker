using Azure.Monitor.OpenTelemetry.Exporter;
using JustTaskTracker.Archival.Functions.Abstractions;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.ExternalProviders.CosmosDB;
using JustTaskTracker.Archival.Functions.Processing;
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
    .AddSingleton<IBoardExportDocumentClient, CosmosBoardExportDocumentClient>();

builder.Build().Run();
