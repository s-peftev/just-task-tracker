using JustTaskTracker.Application.Common.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace JustTaskTracker.Infrastructure.Common.Images;

internal sealed class ImageProcessor : IImageProcessor
{
    /// <inheritdoc />
    public async Task<MemoryStream> ProcessToWebpAsync(Stream source, ImageProcessingSpec spec, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateSpec(spec);

        await using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, ct);

        if (buffer.Length == 0)
            throw new InvalidOperationException("Image stream is empty.");

        buffer.Position = 0;

        using var image = await Image.LoadAsync(buffer, ct);

        if (image.Width > spec.OutputWidth || image.Height > spec.OutputHeight)
        {
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(spec.OutputWidth, spec.OutputHeight),
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center,
            }));
        }

        var output = new MemoryStream();
        var encoder = new WebpEncoder { Quality = spec.WebpQuality };
        await image.SaveAsync(output, encoder, ct);
        output.Position = 0;

        return output;
    }

    private static void ValidateSpec(ImageProcessingSpec spec)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(spec.OutputWidth, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(spec.OutputHeight, 0);

        if (spec.WebpQuality is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(spec), spec.WebpQuality, "WebpQuality must be between 1 and 100.");
    }
}
