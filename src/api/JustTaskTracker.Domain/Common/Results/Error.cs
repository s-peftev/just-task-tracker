using JustTaskTracker.Domain.Common.Enums;

namespace JustTaskTracker.Domain.Common.Results;

/// <summary>
/// Represents a domain error used within the Result pattern.
/// </summary>
/// <param name="Code">Machine-readable error identifier.</param>
/// <param name="Type">Error category used to map to an HTTP status code.</param>
/// <param name="Details">Optional list of contextual or field-level validation messages.</param>
public record Error(string Code, ErrorType Type, IEnumerable<string>? Details = null);
