using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Images;
using JustTaskTracker.Application.Common.Options;

namespace JustTaskTracker.Application.Users.ProfilePhotos;

internal sealed class ProfilePhotoService(
    IBlobStorageService blobStorageService,
    IImageProcessor imageProcessor,
    BlobStorageSettings blobStorageSettings,
    ProfilePhotoProcessingSettings processingSettings) : IProfilePhotoService
{
    private const string WebpContentType = "image/webp";

    private readonly string _containerName = blobStorageSettings.ProfilePhotos!.ContainerName;

    public string BuildOriginalUrl(Guid userId, string version)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        return BuildVersionedUrl(
            blobStorageSettings.ProfilePhotos!.BuildOriginalBlobName(userId),
            version);
    }

    public string BuildThumbnailUrl(Guid userId, string version)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        return BuildVersionedUrl(
            blobStorageSettings.ProfilePhotos!.BuildThumbnailBlobName(userId),
            version);
    }

    public async Task UploadPhotoAsync(Guid userId, Stream source, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(source);

        await using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, ct);

        if (buffer.Length == 0)
            throw new InvalidOperationException("Image stream is empty.");

        buffer.Position = 0;

        var specOriginal = ToSpec(processingSettings.Originals!);
        var specThumbnail = ToSpec(processingSettings.Thumbnails!);

        await using var originalStream = await imageProcessor.ProcessToWebpAsync(buffer, specOriginal, ct);

        buffer.Position = 0;

        await using var thumbnailStream = await imageProcessor.ProcessToWebpAsync(buffer, specThumbnail, ct);

        var originalBlobName = blobStorageSettings.ProfilePhotos!.BuildOriginalBlobName(userId);
        var thumbnailBlobName = blobStorageSettings.ProfilePhotos!.BuildThumbnailBlobName(userId);

        await Task.WhenAll(
            blobStorageService.UploadAsync(_containerName, originalBlobName, originalStream, WebpContentType, ct),
            blobStorageService.UploadAsync(_containerName, thumbnailBlobName, thumbnailStream, WebpContentType, ct));
    }

    public async Task DeleteProfilePhotoAsync(Guid userId, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);

        var originalBlobName = blobStorageSettings.ProfilePhotos!.BuildOriginalBlobName(userId);
        var thumbnailBlobName = blobStorageSettings.ProfilePhotos!.BuildThumbnailBlobName(userId);

        await Task.WhenAll(
            blobStorageService.DeleteAsync(_containerName, originalBlobName, ct),
            blobStorageService.DeleteAsync(_containerName, thumbnailBlobName, ct));
    }

    private static ImageProcessingSpec ToSpec(ProfilePhotoOutputSettings settings) =>
        new(settings.Width, settings.Height, settings.WebpQuality);

    private string BuildVersionedUrl(string blobName, string version) =>
        $"{blobStorageService.GetBlobUri(_containerName, blobName)}?v={Uri.EscapeDataString(version)}";
}
