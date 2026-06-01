using JustTaskTracker.Domain.Common.Enums;

namespace JustTaskTracker.Domain.Common.Results;

/// <summary>
/// Represents a domain error used within the Result pattern.
/// </summary>
/// <param name="Code">Machine-readable error identifier.</param>
/// <param name="Type">Error category used to map to an HTTP status code.</param>
/// <param name="Details">Optional list of contextual or field-level validation messages.</param>
public class Error(string code, ErrorType type, IEnumerable<string>? details = null)
{
    public string Code { get; } = code;
    public ErrorType Type { get; } = type;
    public IEnumerable<string>? Details { get; } = details;

    public override bool Equals(object? obj) =>
        obj is Error other && Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode();

    public static bool operator ==(Error? left, Error? right) => Equals(left, right);
    public static bool operator !=(Error? left, Error? right) => !Equals(left, right);
}
