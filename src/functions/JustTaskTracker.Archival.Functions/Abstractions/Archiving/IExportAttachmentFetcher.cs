using JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

namespace JustTaskTracker.Archival.Functions.Abstractions.Archiving;

public interface IExportAttachmentFetcher
{
    Task<Stream> DownloadAsync(BoardExportAttachmentDto attachment, CancellationToken ct = default);
}
