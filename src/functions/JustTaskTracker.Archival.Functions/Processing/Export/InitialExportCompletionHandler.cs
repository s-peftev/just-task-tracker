using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Abstractions.Processing;
using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Processing.Export;

/// <summary>
/// Applies Cosmos export status transitions for the initial archive export flow
/// (<see cref="BoardExportType.InitialExport"/>). Export options are already stored in the document
/// when the API schedules export; completion only updates status fields.
/// </summary>
public sealed class InitialExportCompletionHandler(
    IBoardExportDocumentClient cosmos,
    IBoardExportStatusNotifyApiClient statusNotifyApiClient)
    : IBoardExportCompletionHandler
{
    public BoardExportType Type => BoardExportType.InitialExport;

    public async Task MarkProcessingAsync(BoardExportContext context, CancellationToken ct = default)
    {
        await cosmos.MarkExportProcessingAsync(context.BoardId, ct);
        await statusNotifyApiClient.NotifyExportStatusChangedAsync(
            context.BoardId,
            BoardExportStatus.Processing,
            ct);
    }

    public async Task MarkCompletedAsync(BoardExportContext context, CancellationToken ct = default)
    {
        await cosmos.CompleteInitialExportAsync(context.BoardId, ct);
        await statusNotifyApiClient.NotifyExportStatusChangedAsync(
            context.BoardId,
            BoardExportStatus.Completed,
            ct);
    }

    public async Task MarkFailedAsync(BoardExportContext context, string errorMessage, CancellationToken ct = default)
    {
        await cosmos.FailInitialExportAsync(context.BoardId, errorMessage, ct);
        await statusNotifyApiClient.NotifyExportStatusChangedAsync(
            context.BoardId,
            BoardExportStatus.Failed,
            ct);
    }
}
