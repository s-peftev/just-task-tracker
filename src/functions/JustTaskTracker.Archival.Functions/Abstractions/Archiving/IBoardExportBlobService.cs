using JustTaskTracker.Archival.Functions.Archiving;

namespace JustTaskTracker.Archival.Functions.Abstractions.Archiving;

public interface IBoardExportBlobService
{
    Task UploadArchiveAsync(Guid boardId, BoardExportArchive archive, CancellationToken ct = default);
}
