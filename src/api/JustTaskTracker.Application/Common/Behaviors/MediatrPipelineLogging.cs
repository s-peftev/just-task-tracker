namespace JustTaskTracker.Application.Common.Behaviors;

/// <summary>
/// Marker type for a unified <see cref="Microsoft.Extensions.Logging.ILogger"/> category
/// across all MediatR pipeline logging, so log levels can be configured in one place.
/// </summary>
public sealed class MediatrPipelineLogging;
