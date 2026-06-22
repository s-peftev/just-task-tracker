using System.Collections.Concurrent;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using JustTaskTracker.Application.Common.Interfaces.ExternalProviders;
using JustTaskTracker.Application.Common.Models;

namespace JustTaskTracker.Infrastructure.Common.ExternalProviders;

internal sealed class AzureBlobStorageService(BlobServiceClient blobServiceClient) : IBlobStorageService
{
    private readonly ConcurrentDictionary<string, BlobContainerClient> _containerClients = new(StringComparer.Ordinal);

    public async Task UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        if (content.CanSeek)
            content.Position = 0;

        await GetBlobClient(containerName, blobName).UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
            },
            ct);
    }

    public async Task<BlobContent> DownloadAsync(string containerName, string blobName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        var response = await GetBlobClient(containerName, blobName).DownloadStreamingAsync(cancellationToken: ct);
        var details = response.Value.Details;

        return new BlobContent(
            response.Value.Content,
            string.IsNullOrWhiteSpace(details.ContentType) ? "application/octet-stream" : details.ContentType,
            details.ContentLength);
    }

    public async Task MoveAsync(string containerName, string sourceBlobName, string destinationBlobName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceBlobName);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationBlobName);

        if (string.Equals(sourceBlobName, destinationBlobName, StringComparison.Ordinal))
            throw new ArgumentException("Source and destination blob names must differ.");

        var sourceClient = GetBlobClient(containerName, sourceBlobName);
        var destinationClient = GetBlobClient(containerName, destinationBlobName);

        var copyOperation = await destinationClient.StartCopyFromUriAsync(
            sourceClient.Uri,
            new BlobCopyFromUriOptions
            {
                DestinationConditions = new BlobRequestConditions { IfNoneMatch = ETag.All },
            },
            ct);

        await copyOperation.WaitForCompletionAsync(cancellationToken: ct);

        await sourceClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
    }

    public async Task DeleteAsync(string containerName, string blobName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        await GetBlobClient(containerName, blobName)
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
    }

    private BlobClient GetBlobClient(string containerName, string blobName) =>
        GetContainerClient(containerName).GetBlobClient(blobName);

    private BlobContainerClient GetContainerClient(string containerName) =>
        _containerClients.GetOrAdd(
            containerName,
            name => blobServiceClient.GetBlobContainerClient(name));
}
