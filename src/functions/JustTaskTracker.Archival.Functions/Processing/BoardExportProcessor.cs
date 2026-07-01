using JustTaskTracker.Archival.Functions.Abstractions.Archiving;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Abstractions.Processing;
using JustTaskTracker.Archival.Functions.Archiving.Summary;
using JustTaskTracker.Archival.Functions.Contracts.Messaging;
using JustTaskTracker.Archival.Functions.Processing.Export;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Archival.Functions.Processing;

public class BoardExportProcessor(
    ExportContextResolver exportContextResolver,
    BoardExportCompletionHandlerRegistry completionHandlerRegistry,
    IBoardExportDocumentClient boardExportDocumentClient,
    IBoardExportDataApiClient boardExportDataApiClient,
    IBoardArchiveBuilder archiveBuilder,
    ILogger<BoardExportProcessor> logger)
    : IBoardExportProcessor
{
    public async Task RunAsync(BoardExportMessage message, CancellationToken ct = default)
    {
        var exportInfo = await boardExportDocumentClient.GetBoardExportInfoAsync(message.BoardId, ct)
            ?? throw new InvalidOperationException($"Export document not found for board {message.BoardId}.");

        // get an appropriate export policy (for initial export or re-export) according to BoardExportMessage type
        var exportContext = exportContextResolver.Resolve(message, exportInfo);
        var completionHandler = completionHandlerRegistry.Get(message.Type);

        if (exportContext.ShouldSkip)
        {
            logger.LogInformation(
                "Skipping export. BoardId={BoardId}, Type={ExportType}, Reason={Reason}",
                message.BoardId,
                message.Type,
                exportContext.SkipReason);

            return;
        }

        if (exportContext.Options is not { } exportOptions)
        {
            throw new InvalidOperationException(
                $"Export options are missing for board {exportContext.BoardId}.");
        }

        try
        {
            await completionHandler.MarkProcessingAsync(exportContext, ct);

            var exportData = await boardExportDataApiClient.GetExportDataAsync(
                exportContext.BoardId,
                exportOptions,
                ct);

            // TODO: resolve summary formats from exportOptions when PDF export is supported.
            IReadOnlyList<BoardExportSummaryFormat> summaryFormats = [BoardExportSummaryFormat.Json];

            await using var archive = await archiveBuilder.BuildAsync(exportData, summaryFormats, ct);

            // TODO: upload archive.Content to blob storage using archive.FileName

            await completionHandler.MarkCompletedAsync(exportContext, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Board export failed. BoardId={BoardId}, Type={ExportType}",
                message.BoardId,
                message.Type);

            await completionHandler.MarkFailedAsync(exportContext, ex.Message, ct);

            throw;
        }
    }
}
