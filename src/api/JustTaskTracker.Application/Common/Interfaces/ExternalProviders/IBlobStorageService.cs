using JustTaskTracker.Application.Common.Models;

namespace JustTaskTracker.Application.Common.Interfaces.ExternalProviders;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads content to the configured attachments container.
    /// </summary>
    /// <remarks>
    /// Upload fails with HTTP 409 from storage when <paramref name="blobName"/> already exists.
    /// When <paramref name="content"/> is seekable, its position is reset to the start before upload.
    /// </remarks>
    Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Opens a read stream for the blob identified by <paramref name="blobName"/>.
    /// </summary>
    /// <remarks>
    /// The caller owns the returned <see cref="BlobContent.Content"/> until it is passed to ASP.NET file
    /// result helpers, which take over disposal after the response completes. If processing fails after
    /// download but before returning a file result, dispose <see cref="BlobContent.Content"/> explicitly.
    /// </remarks>
    Task<BlobContent> DownloadAsync(string blobName, CancellationToken ct = default);

    /// <summary>
    /// Moves a blob to <paramref name="destinationBlobName"/> via server-side copy, then deletes the source.
    /// </summary>
    /// <remarks>
    /// Fails when the source blob does not exist or the destination blob already exists.
    /// </remarks>
    Task MoveToDeletedAsync(string sourceBlobName, string destinationBlobName, CancellationToken ct = default);

    Task DeleteAsync(string blobName, CancellationToken ct = default);
}
