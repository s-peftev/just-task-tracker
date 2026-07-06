using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Abstractions.Processing;
using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Processing.Export;

/// <summary>
/// Applies Cosmos status transitions for the re-export flow (<see cref="BoardExportType.ReExport"/>).
/// On success, promotes <c>reExportOptions</c> to <c>exportOptions</c> and clears re-export fields.
/// </summary>
public sealed class ReExportCompletionHandler(
    IBoardExportDocumentClient cosmos,
    IBoardExportStatusNotifyApiClient statusNotifyApiClient)
    : IBoardExportCompletionHandler
{
    public BoardExportType Type => BoardExportType.ReExport;

    public async Task MarkProcessingAsync(BoardExportContext context, CancellationToken ct = default)
    {
        await cosmos.MarkReExportProcessingAsync(context.BoardId, ct);
        await statusNotifyApiClient.NotifyReExportStatusChangedAsync(
            context.BoardId,
            BoardExportStatus.Processing,
            ct: ct);
    }

    public async Task MarkCompletedAsync(BoardExportContext context, CancellationToken ct = default)
    {
        if (context.Options is not { } promotedExportOptions)
        {
            throw new InvalidOperationException(
                $"Re-export options are missing for board {context.BoardId}.");
        }

        await cosmos.CompleteReExportAsync(context.BoardId, promotedExportOptions, ct);
        await statusNotifyApiClient.NotifyReExportStatusChangedAsync(
            context.BoardId,
            BoardExportStatus.None,
            promotedExportOptions,
            ct);
    }

    public async Task MarkFailedAsync(BoardExportContext context, string errorMessage, CancellationToken ct = default)
    {
        await cosmos.FailReExportAsync(context.BoardId, errorMessage, ct);
        await statusNotifyApiClient.NotifyReExportStatusChangedAsync(
            context.BoardId,
            BoardExportStatus.Failed,
            ct: ct);
    }
}
