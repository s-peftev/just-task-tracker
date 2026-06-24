namespace JustTaskTracker.Application.Common.Images;

/// <summary>
/// Target dimensions and encoding options for WebP image processing.
/// </summary>
public sealed record ImageProcessingSpec(int OutputWidth, int OutputHeight, int WebpQuality);
