namespace JustTaskTracker.Application.Common.Images;

public interface IImageProcessor
{
    /// <summary>
    /// Reads an image from <paramref name="source"/>, applies <paramref name="spec"/>, and returns a new WebP stream positioned at 0.
    /// Images that already fit within the target dimensions are not upscaled.
    /// Caller owns the returned <see cref="MemoryStream"/> and must dispose it.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="spec"/> contains invalid dimensions or quality.</exception>
    /// <exception cref="InvalidOperationException">The source stream is empty.</exception>
    Task<MemoryStream> ProcessToWebpAsync(Stream source, ImageProcessingSpec spec, CancellationToken ct = default);
}
