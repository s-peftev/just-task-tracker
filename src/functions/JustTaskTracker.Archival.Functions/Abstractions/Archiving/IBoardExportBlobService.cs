using JustTaskTracker.Archival.Functions.Archiving;

namespace JustTaskTracker.Archival.Functions.Abstractions.Archiving;

public interface IBoardExportBlobService
{
    Task<Stream> DownloadAttachmentAsync(string blobName, CancellationToken ct = default);

    Task UploadArchiveAsync(Guid boardId, BoardExportArchive archive, CancellationToken ct = default);
}
