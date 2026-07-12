namespace JustTaskTracker.API.Filters;

/// <summary>
/// Skips <see cref="ApiResponseEnvelopeFilter"/> wrapping for the marked controller or action.
/// Used for endpoints that must return raw HTTP responses (e.g. Stripe webhooks).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipApiResponseEnvelopeAttribute : Attribute;
