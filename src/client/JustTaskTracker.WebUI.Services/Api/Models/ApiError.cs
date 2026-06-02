using JustTaskTracker.WebUI.Domain.Common;

namespace JustTaskTracker.WebUI.Services.Api.Models;

public record ApiError(string Code, ApiErrorType Type, IReadOnlyList<string>? Details);
