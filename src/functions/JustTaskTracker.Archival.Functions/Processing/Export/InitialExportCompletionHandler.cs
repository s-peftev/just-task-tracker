using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Abstractions.Processing;
using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Processing.Export;

/// <summary>
/// Applies Cosmos export status transitions for the initial archive export flow
/// (<see cref="BoardExportType.InitialExport"/>). Export options are already stored in the document
/// when the API schedules export; completion only updates status fields.
/// </summary>
public sealed class InitialExportCompletionHandler(IBoardExportDocumentClient cosmos)
    : IBoardExportCompletionHandler
{
    public BoardExportType Type => BoardExportType.InitialExport;

    public Task MarkProcessingAsync(BoardExportContext context, CancellationToken ct = default) =>
        cosmos.MarkExportProcessingAsync(context.BoardId, ct);

    public Task MarkCompletedAsync(BoardExportContext context, CancellationToken ct = default) =>
        cosmos.CompleteInitialExportAsync(context.BoardId, ct);

    public Task MarkFailedAsync(BoardExportContext context, string errorMessage, CancellationToken ct = default) =>
        cosmos.FailInitialExportAsync(context.BoardId, errorMessage, ct);
}
