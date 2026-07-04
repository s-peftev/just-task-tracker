using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using JustTaskTracker.Archival.Functions.Abstractions.Archiving;
using JustTaskTracker.Archival.Functions.Archiving;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.Blob;

public sealed class BoardExportBlobService(
    BlobServiceClient blobServiceClient,
    IOptions<BlobStorageOptions> options,
    ILogger<BoardExportBlobService> logger) : IBoardExportBlobService
{
    private const string ZipContentType = "application/zip";

    public async Task UploadArchiveAsync(Guid boardId, BoardExportArchive archive, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(archive);

        var blobOptions = options.Value;
        blobOptions.Validate();

        var blobName = $"{boardId:D}/{archive.FileName}";

        if (archive.Content.CanSeek)
            archive.Content.Position = 0;

        await blobServiceClient
            .GetBlobContainerClient(blobOptions.ArchivesContainerName)
            .GetBlobClient(blobName)
            .UploadAsync(
                archive.Content,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = ZipContentType },
                },
                ct);

        logger.LogInformation(
            "Board export archive uploaded. BoardId={BoardId}, Container={Container}, BlobName={BlobName}",
            boardId,
            blobOptions.ArchivesContainerName,
            blobName);
    }
}
