using JustTaskTracker.WebUI.Services.Api.Models;
using JustTaskTracker.WebUI.Services.Exceptions;
using Refit;

namespace JustTaskTracker.WebUI.Services.Api;

/// <summary>
/// Shared helpers that unwrap the <see cref="ApiEnvelope{T}"/> returned by the API,
/// throwing <see cref="ApiServiceException"/> with the server-provided error on failure.
/// </summary>
internal static class ApiResponseGuard
{
    public static T Unwrap<T>(IApiResponse<ApiEnvelope<T>> response)
    {
        var envelope = response.Content;

        if (!response.IsSuccessStatusCode || envelope is { Success: false } || envelope is null || envelope.Data is null)
            throw ToException(response, envelope);

        return envelope.Data;
    }

    public static void EnsureSuccess<T>(IApiResponse<ApiEnvelope<T>> response)
    {
        if (!response.IsSuccessStatusCode)
            throw ToException(response, response.Content);
    }

    private static ApiServiceException ToException<T>(IApiResponse<ApiEnvelope<T>> response, ApiEnvelope<T>? envelope) =>
        new(response.StatusCode, envelope?.Error, envelope?.Error?.Code ?? "Unexpected API error");
}
