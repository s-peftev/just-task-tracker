using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using JustTaskTracker.Application.Common.Interfaces.ExternalProviders;
using JustTaskTracker.Application.Common.Models;
using JustTaskTracker.Infrastructure.Common.Options;

namespace JustTaskTracker.Infrastructure.Common.ExternalProviders;

internal sealed class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        BlobStorageContainersOptions containersOptions)
    {
        if (string.IsNullOrWhiteSpace(containersOptions.TaskAttachments))
        {
            throw new InvalidOperationException(
                $"{nameof(BlobStorageContainersOptions.TaskAttachments)} is not configured.");
        }

        _containerClient = blobServiceClient.GetBlobContainerClient(containersOptions.TaskAttachments);
    }

    public async Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        if (content.CanSeek)
            content.Position = 0;

        await GetBlobClient(blobName).UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
            },
            ct);
    }

    public async Task<BlobContent> DownloadAsync(string blobName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        var response = await GetBlobClient(blobName).DownloadStreamingAsync(cancellationToken: ct);
        var details = response.Value.Details;

        return new BlobContent(
            response.Value.Content,
            string.IsNullOrWhiteSpace(details.ContentType) ? "application/octet-stream" : details.ContentType,
            details.ContentLength);
    }

    public async Task DeleteAsync(string blobName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        await GetBlobClient(blobName).DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
    }

    private BlobClient GetBlobClient(string blobName) => _containerClient.GetBlobClient(blobName);
}
