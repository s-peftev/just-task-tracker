using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Messaging;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Infrastructure.Boards.Jobs;

internal static class BoardExportJobOperations
{
    internal static async Task EnqueueAndMarkPendingAsync(
        IBoardExportQueueSender queueSender,
        ILogger logger,
        BoardExportStatusInfo info,
        BoardExportType exportType,
        Func<Guid, CancellationToken, Task> markPendingAsync,
        Func<BoardExportStatusChangedNotification, CancellationToken, Task> notifyStatusChangedAsync,
        CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();
        var message = new BoardExportMessage(info.BoardId, exportType, correlationId);

        try
        {
            await markPendingAsync(info.BoardId, ct);
            await queueSender.SendAsync(message, ct);

            await notifyStatusChangedAsync(
                new BoardExportStatusChangedNotification(info.BoardId, BoardExportStatus.Pending),
                ct);

            logger.LogInformation(
                "Enqueued board export. BoardId={BoardId}, ExportType={ExportType}, CorrelationId={CorrelationId}",
                info.BoardId,
                exportType,
                correlationId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to enqueue board export. BoardId={BoardId}, ExportType={ExportType}, CorrelationId={CorrelationId}",
                info.BoardId,
                exportType,
                correlationId);
        }
    }
}
