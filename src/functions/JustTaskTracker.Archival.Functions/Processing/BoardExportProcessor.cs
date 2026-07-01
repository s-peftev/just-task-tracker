using JustTaskTracker.Archival.Functions.Abstractions;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Contracts.Messaging;
using JustTaskTracker.Archival.Functions.Processing.Export;

namespace JustTaskTracker.Archival.Functions.Processing;

public class BoardExportProcessor(
    ExportContextResolver exportContextResolver,
    IBoardExportDocumentClient boardExportDocumentClient)
    : IBoardExportProcessor
{
    public async Task RunAsync(BoardExportMessage message, CancellationToken ct = default)
    {
        var exportInfo = await boardExportDocumentClient.GetBoardExportInfoAsync(message.BoardId, ct)
            ?? throw new InvalidOperationException($"Export document not found for board {message.BoardId}.");

        var exportContext = exportContextResolver.Resolve(message, exportInfo);
    }
}
