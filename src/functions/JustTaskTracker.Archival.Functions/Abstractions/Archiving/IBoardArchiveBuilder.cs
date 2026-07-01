using JustTaskTracker.Archival.Functions.Archiving;
using JustTaskTracker.Archival.Functions.Archiving.Summary;
using JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

namespace JustTaskTracker.Archival.Functions.Abstractions.Archiving;

public interface IBoardArchiveBuilder
{
    Task<BoardExportArchive> BuildAsync(
        BoardExportDataDto data,
        IReadOnlyList<BoardExportSummaryFormat> summaryFormats,
        CancellationToken ct = default);
}
