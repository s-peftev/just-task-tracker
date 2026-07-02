using Hangfire;
using JustTaskTracker.Application.Boards.Jobs;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Messaging;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Infrastructure.Boards.Jobs;

/// <summary>
/// Hangfire recurring job that scans Cosmos DB for board export documents in
/// <see cref="BoardExportStatus.Requested"/> or <see cref="BoardExportStatus.Failed"/> states
/// and enqueues a <see cref="BoardExportMessage"/> for each actionable document.
/// Status is updated to <see cref="BoardExportStatus.Pending"/> after the message is sent.
/// </summary>
[AutomaticRetry(Attempts = 0)]
internal sealed class BoardExportSchedulerJob(
    IBoardExportService boardExportService,
    IBoardExportQueueSender queueSender,
    BoardExportSchedulerOptions options,
    IDateTimeProvider dateTimeProvider,
    ILogger<BoardExportSchedulerJob> logger) : IBoardExportSchedulerJob
{
    public async Task RunAsync(CancellationToken ct)
    {
        var cooldownThreshold = dateTimeProvider.UtcNow.AddMinutes(-options.FailedRetryCooldownMinutes);

        logger.LogInformation(
            "Board export scheduler started. BatchSize={BatchSize}, CooldownThreshold={CooldownThreshold:O}",
            options.DocumentBatchSize,
            cooldownThreshold);

        var processed = 0;

        await foreach (var info in boardExportService.ScanActionableAsync(options.DocumentBatchSize, cooldownThreshold, ct))
        {
            if (IsActionable(info.ExportStatus))
            {
                await EnqueueAndMarkPendingAsync(
                    info.BoardId,
                    BoardExportType.InitialExport,
                    updateStatus: async (boardId, correlationId) =>
                    {
                        await boardExportService.UpdateExportStatusAsync(boardId, BoardExportStatus.Pending, null, ct);
                    },
                    ct);
            }

            if (IsActionable(info.ReExportStatus))
            {
                await EnqueueAndMarkPendingAsync(
                    info.BoardId,
                    BoardExportType.ReExport,
                    updateStatus: async (boardId, correlationId) =>
                    {
                        await boardExportService.UpdateReExportStatusAsync(boardId, BoardExportStatus.Pending, null, ct);
                    },
                    ct);
            }

            processed++;
        }

        logger.LogInformation("Board export scheduler finished. Processed={Processed}", processed);
    }

    private async Task EnqueueAndMarkPendingAsync(Guid boardId, BoardExportType exportType, Func<Guid, string, Task> updateStatus, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid().ToString();
        var message = new BoardExportMessage(boardId, exportType, correlationId);

        try
        {
            await queueSender.SendAsync(message, ct);
            await updateStatus(boardId, correlationId);

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

            // Do not rethrow — continue processing remaining documents.
        }
    }

    private static bool IsActionable(BoardExportStatus status) =>
        status is BoardExportStatus.Requested or BoardExportStatus.Failed;
}
