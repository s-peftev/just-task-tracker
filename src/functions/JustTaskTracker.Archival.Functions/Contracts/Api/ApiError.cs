namespace JustTaskTracker.Archival.Functions.Contracts.Api;

public record ApiError(string Code, IReadOnlyList<string>? Details);
