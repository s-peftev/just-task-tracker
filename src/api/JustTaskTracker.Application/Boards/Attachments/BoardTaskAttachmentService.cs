using JustTaskTracker.Application.Common.Interfaces.ExternalProviders;
using JustTaskTracker.Application.Common.Models;
using JustTaskTracker.Application.Common.Options;

namespace JustTaskTracker.Application.Boards.Attachments;

internal sealed class BoardTaskAttachmentService(
    IBlobStorageService blobStorageService,
    BlobStorageSettings blobStorageSettings) : IBoardTaskAttachmentService
{
    private readonly string _containerName = blobStorageSettings.TaskAttachments.ContainerName
        is { Length: > 0 } containerName
            ? containerName
            : throw new InvalidOperationException(
                $"{nameof(blobStorageSettings.TaskAttachments.ContainerName)} is not configured.");

    public string BuildActiveBlobName(Guid boardTaskId, Guid blobId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardTaskId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(blobId, Guid.Empty);

        return blobStorageSettings.TaskAttachments.BuildActiveBlobName(boardTaskId, blobId);
    }

    public string ToDeletedBlobName(string activeBlobName) =>
        blobStorageSettings.TaskAttachments.ToDeletedBlobName(activeBlobName);

    public Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken ct = default) =>
        blobStorageService.UploadAsync(_containerName, blobName, content, contentType, ct);

    public Task<BlobContent> DownloadAsync(string blobName, CancellationToken ct = default) =>
        blobStorageService.DownloadAsync(_containerName, blobName, ct);

    public Task MoveToDeletedAsync(string sourceBlobName, string destinationBlobName, CancellationToken ct = default) =>
        blobStorageService.MoveAsync(_containerName, sourceBlobName, destinationBlobName, ct);

    public Task DeleteAsync(string blobName, CancellationToken ct = default) =>
        blobStorageService.DeleteAsync(_containerName, blobName, ct);
}
