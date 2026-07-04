namespace JustTaskTracker.Archival.Functions.Contracts.Api;

public record ApiEnvelope<T>(bool Success, T? Data, ApiError? Error);
