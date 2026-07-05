using Hangfire;
using JustTaskTracker.Application.Boards.Jobs;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Boards.Enums;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Infrastructure.Boards.Jobs;

/// <summary>
/// Hangfire recurring job that scans Cosmos DB for board export documents in
/// <see cref="BoardExportStatus.Requested"/> state and enqueues a message for each.
/// Status is updated to <see cref="BoardExportStatus.Pending"/> after the message is sent.
/// </summary>
[AutomaticRetry(Attempts = 0)]
internal sealed class BoardExportSchedulerJob(
    IBoardExportService boardExportService,
    IBoardExportQueueSender queueSender,
    IBoardExportStatusNotifier exportStatusNotifier,
    BoardExportSchedulerOptions options,
    ILogger<BoardExportSchedulerJob> logger) : IBoardExportSchedulerJob
{
    public async Task RunAsync(CancellationToken ct)
    {
        logger.LogInformation(
            "Board export scheduler started. BatchSize={BatchSize}",
            options.DocumentBatchSize);

        var processed = 0;

        await foreach (var info in boardExportService.ScanForRequestedExportStatusesAsync(options.DocumentBatchSize, ct))
        {
            if (info.ExportStatus == BoardExportStatus.Requested)
            {
                await BoardExportJobOperations.EnqueueAndMarkPendingAsync(
                    queueSender,
                    logger,
                    info,
                    BoardExportType.InitialExport,
                    (boardId, token) => boardExportService.UpdateExportStatusAsync(boardId, BoardExportStatus.Pending, null, token),
                    exportStatusNotifier.NotifyExportStatusChangedAsync,
                    ct);
            }

            if (info.ReExportStatus == BoardExportStatus.Requested)
            {
                await BoardExportJobOperations.EnqueueAndMarkPendingAsync(
                    queueSender,
                    logger,
                    info,
                    BoardExportType.ReExport,
                    (boardId, token) => boardExportService.UpdateReExportStatusAsync(boardId, BoardExportStatus.Pending, null, token),
                    exportStatusNotifier.NotifyReExportStatusChangedAsync,
                    ct);
            }

            processed++;
        }

        logger.LogInformation("Board export scheduler finished. Processed={Processed}", processed);
    }
}
