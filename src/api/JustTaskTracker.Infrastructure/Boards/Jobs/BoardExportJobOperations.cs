using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Messaging;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Infrastructure.Boards.Jobs;

internal static class BoardExportJobOperations
{
    internal static async Task EnqueueAndMarkPendingAsync(
        IBoardExportService boardExportService,
        IBoardExportQueueSender queueSender,
        ILogger logger,
        Guid boardId,
        BoardExportType exportType,
        Func<Guid, CancellationToken, Task> markPendingAsync,
        CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();
        var message = new BoardExportMessage(boardId, exportType, correlationId);

        try
        {
            await markPendingAsync(boardId, ct);
            await queueSender.SendAsync(message, ct);

            logger.LogInformation(
                "Enqueued board export. BoardId={BoardId}, ExportType={ExportType}, CorrelationId={CorrelationId}",
                boardId,
                exportType,
                correlationId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to enqueue board export. BoardId={BoardId}, ExportType={ExportType}, CorrelationId={CorrelationId}",
                boardId,
                exportType,
                correlationId);
        }
    }
}
