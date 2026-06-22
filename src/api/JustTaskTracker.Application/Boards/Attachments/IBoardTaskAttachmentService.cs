using JustTaskTracker.Application.Common.Models;

namespace JustTaskTracker.Application.Boards.Attachments;

/// <summary>
/// Task attachment blob paths and storage operations for the configured attachments container.
/// </summary>
public interface IBoardTaskAttachmentService
{
    /// <summary>
    /// Builds a blob name under the active attachments folder.
    /// </summary>
    string BuildActiveBlobName(Guid boardTaskId, Guid blobId);

    /// <summary>
    /// Maps an active attachment blob name to its soft-deleted location.
    /// </summary>
    string ToDeletedBlobName(string activeBlobName);

    /// <summary>
    /// Uploads content to the attachments container.
    /// </summary>
    /// <remarks>
    /// Upload fails with HTTP 409 from storage when <paramref name="blobName"/> already exists.
    /// When <paramref name="content"/> is seekable, its position is reset to the start before upload.
    /// </remarks>
    Task UploadAsync(string blobName, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Opens a read stream for the attachment blob identified by <paramref name="blobName"/>.
    /// </summary>
    Task<BlobContent> DownloadAsync(string blobName, CancellationToken ct = default);

    /// <summary>
    /// Moves an attachment blob to <paramref name="destinationBlobName"/> via server-side copy, then deletes the source.
    /// </summary>
    Task MoveToDeletedAsync(string sourceBlobName, string destinationBlobName, CancellationToken ct = default);

    /// <summary>
    /// Deletes the attachment blob identified by <paramref name="blobName"/>.
    /// </summary>
    Task DeleteAsync(string blobName, CancellationToken ct = default);
}
