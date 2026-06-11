using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Services.Abstractions.Auth;
using JustTaskTracker.WebUI.Services.Api;
using JustTaskTracker.WebUI.Services.Api.Models;
using JustTaskTracker.WebUI.Services.Exceptions;
using Refit;
using System.Net;

namespace JustTaskTracker.WebUI.Services.Auth;

internal class AuthApiService(IAuthApi api) : IAuthApiService
{
    public async Task<UserWithRolesDto> LoginAsync(CancellationToken ct = default)
    {
        var response = await api.LoginAsync(ct);

        return UnwrapResponse(response);
    }

    public async Task<UserWithRolesDto?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        var response = await api.GetCurrentUserAsync(ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        return UnwrapResponse(response);
    }

    private static UserWithRolesDto UnwrapResponse(IApiResponse<ApiEnvelope<UserWithRolesDto>> response)
    {
        var envelope = response.Content;

        if (!response.IsSuccessStatusCode || envelope is { Success: false } || envelope?.Data is null)
        {
            throw new ApiServiceException(
                response.StatusCode,
                envelope?.Error,
                ApiResponseGuard.ResolveMessage(envelope?.Error));
        }

        return envelope.Data;
    }
}
