namespace JustTaskTracker.Application.Users.ProfilePhotos;

/// <summary>
/// Profile photo URL building and blob upload.
/// </summary>
public interface IProfilePhotoService
{
    /// <summary>
    /// Returns the public URL for the user's original profile photo blob with cache-busting version.
    /// </summary>
    string BuildOriginalUrl(Guid userId, string version);

    /// <summary>
    /// Returns the public URL for the user's thumbnail profile photo blob with cache-busting version.
    /// </summary>
    string BuildThumbnailUrl(Guid userId, string version);

    /// <summary>
    /// Processes <paramref name="source"/> into original and thumbnail WebP blobs and uploads them for <paramref name="userId"/>.
    /// Existing blobs at the configured paths are overwritten.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="userId"/> is empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The source stream is empty.</exception>
    Task UploadPhotoAsync(Guid userId, Stream source, CancellationToken ct = default);
}
