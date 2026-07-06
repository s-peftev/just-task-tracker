using Hangfire;
using JustTaskTracker.Application.Boards.Jobs;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Enums;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Infrastructure.Boards.Jobs;

/// <summary>
/// Hangfire recurring job that re-enqueues board exports stuck in
/// <see cref="BoardExportStatus.Failed"/>, <see cref="BoardExportStatus.Pending"/>,
/// or <see cref="BoardExportStatus.Processing"/> states after the configured cooldowns elapse.
/// </summary>
[AutomaticRetry(Attempts = 0)]
internal sealed class BoardExportRecoverySchedulerJob(
    IBoardExportService boardExportService,
    IBoardExportQueueSender queueSender,
    IBoardExportStatusNotifier exportStatusNotifier,
    BoardExportRecoverySchedulerOptions options,
    IDateTimeProvider dateTimeProvider,
    ILogger<BoardExportRecoverySchedulerJob> logger) : IBoardExportRecoverySchedulerJob
{
    public async Task RunAsync(CancellationToken ct)
    {
        var failedCooldownThreshold = dateTimeProvider.UtcNow.AddMinutes(-options.FailedRetryCooldownMinutes);
        var staleCooldownThreshold = dateTimeProvider.UtcNow.AddMinutes(-options.StaleCooldownMinutes);

        logger.LogInformation(
            "Board export recovery scheduler started. BatchSize={BatchSize}, FailedCooldownThreshold={FailedCooldownThreshold:O}, StaleCooldownThreshold={StaleCooldownThreshold:O}",
            options.DocumentBatchSize,
            failedCooldownThreshold,
            staleCooldownThreshold);

        var processed = 0;

        await foreach (var info in boardExportService.ScanForFailedExportStatusesAsync(
                           options.DocumentBatchSize,
                           failedCooldownThreshold,
                           ct))
        {
            await RecoverExportAsync(info, ct);
            processed++;
        }

        await foreach (var info in boardExportService.ScanForStaleExportStatusesAsync(
                           options.DocumentBatchSize,
                           staleCooldownThreshold,
                           ct))
        {
            await RecoverExportAsync(info, ct);
            processed++;
        }

        logger.LogInformation("Board export recovery scheduler finished. Processed={Processed}", processed);
    }

    private async Task RecoverExportAsync(BoardExportStatusInfo info, CancellationToken ct)
    {
        if (ShouldRecover(info.ExportStatus))
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

        if (ShouldRecover(info.ReExportStatus))
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
    }

    private static bool ShouldRecover(BoardExportStatus status) =>
        status is BoardExportStatus.Pending or BoardExportStatus.Processing or BoardExportStatus.Failed;
}
