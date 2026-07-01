using System.IO.Compression;
using JustTaskTracker.Archival.Functions.Archiving.Summary;
using JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

namespace JustTaskTracker.Archival.Functions.Abstractions.Archiving;

public interface IBoardExportSummaryWriter
{
    BoardExportSummaryFormat Format { get; }

    Task WriteAsync(ZipArchive archive, BoardExportDataDto data, CancellationToken ct = default);
}
