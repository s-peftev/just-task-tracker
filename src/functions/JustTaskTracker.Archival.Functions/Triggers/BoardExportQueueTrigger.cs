using JustTaskTracker.Archival.Functions.Abstractions;
using JustTaskTracker.Archival.Functions.Constants;
using JustTaskTracker.Archival.Functions.Contracts.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Archival.Functions.Triggers;

public class BoardExportQueueTrigger(
    ILogger<BoardExportQueueTrigger> logger,
    IBoardExportProcessor boardExportProcessor)
{
    [Function(nameof(BoardExportQueueTrigger))]
    public async Task Run(
        [ServiceBusTrigger(ServiceBusQueueNames.BoardArchivingQueue)]
        BoardExportMessage message,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Board export started. BoardId={BoardId}, ExportType={ExportType}, CorrelationId={CorrelationId}",
            message.BoardId,
            message.Type,
            message.CorrelationId);

        try
        {
            await boardExportProcessor.RunAsync(message, ct);

            logger.LogInformation(
                "Board export finished. BoardId={BoardId}, ExportType={ExportType}, CorrelationId={CorrelationId}",
                message.BoardId,
                message.Type,
                message.CorrelationId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Board export failed. BoardId={BoardId}, ExportType={ExportType}, CorrelationId={CorrelationId}",
                message.BoardId,
                message.Type,
                message.CorrelationId);

            throw;
        }
    }
}