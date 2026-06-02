using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface IAuthApi
{
    [Post("/api/auth/login")]
    Task<IApiResponse<ApiEnvelope<UserWithRolesDto>>> LoginAsync(CancellationToken ct = default);

    [Get("/api/auth/me")]
    Task<IApiResponse<ApiEnvelope<UserWithRolesDto>>> GetCurrentUserAsync(CancellationToken ct = default);
}
