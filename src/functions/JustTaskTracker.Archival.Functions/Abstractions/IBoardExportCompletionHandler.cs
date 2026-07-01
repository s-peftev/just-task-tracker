using JustTaskTracker.Archival.Functions.Contracts.Enums;
using JustTaskTracker.Archival.Functions.Processing.Export;

namespace JustTaskTracker.Archival.Functions.Abstractions;

public interface IBoardExportCompletionHandler
{
    BoardExportType Type { get; }

    Task MarkProcessingAsync(BoardExportContext context, CancellationToken ct = default);

    Task MarkCompletedAsync(BoardExportContext context, CancellationToken ct = default);

    Task MarkFailedAsync(BoardExportContext context, string errorMessage, CancellationToken ct = default);
}
