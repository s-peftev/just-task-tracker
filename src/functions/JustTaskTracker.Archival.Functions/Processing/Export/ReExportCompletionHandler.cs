using JustTaskTracker.Archival.Functions.Abstractions;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Processing.Export;

/// <summary>
/// Applies Cosmos status transitions for the re-export flow (<see cref="BoardExportType.ReExport"/>).
/// On success, promotes <c>reExportOptions</c> to <c>exportOptions</c> and clears re-export fields.
/// </summary>
public sealed class ReExportCompletionHandler(IBoardExportDocumentClient cosmos)
    : IBoardExportCompletionHandler
{
    public BoardExportType Type => BoardExportType.ReExport;

    public Task MarkProcessingAsync(BoardExportContext context, CancellationToken ct = default) =>
        cosmos.MarkReExportProcessingAsync(context.BoardId, ct);

    public Task MarkCompletedAsync(BoardExportContext context, CancellationToken ct = default)
    {
        if (context.Options is not { } promotedExportOptions)
        {
            throw new InvalidOperationException(
                $"Re-export options are missing for board {context.BoardId}.");
        }

        return cosmos.CompleteReExportAsync(context.BoardId, promotedExportOptions, ct);
    }

    public Task MarkFailedAsync(BoardExportContext context, string errorMessage, CancellationToken ct = default) =>
        cosmos.FailReExportAsync(context.BoardId, errorMessage, ct);
}
