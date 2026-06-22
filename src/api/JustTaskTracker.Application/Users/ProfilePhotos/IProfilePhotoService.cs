namespace JustTaskTracker.Application.Users.ProfilePhotos;

/// <summary>
/// Builds profile photo storage paths and public URLs.
/// </summary>
public interface IProfilePhotoService
{
    /// <summary>
    /// Returns the public URL for the user's original profile photo blob.
    /// </summary>
    string BuildOriginalUrl(Guid userId);

    /// <summary>
    /// Returns the public URL for the user's thumbnail profile photo blob.
    /// </summary>
    string BuildThumbnailUrl(Guid userId);
}
