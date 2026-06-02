namespace JustTaskTracker.WebUI.Services.Api.Models;

public record ApiEnvelope<T>(bool Success, T? Data, ApiError? Error);
