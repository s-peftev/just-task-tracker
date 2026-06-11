using System.Text.Json;
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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static T Unwrap<T>(IApiResponse<ApiEnvelope<T>> response)
    {
        var envelope = response.Content;

        if (response.IsSuccessStatusCode &&
            envelope is { Success: true, Data: not null })
        {
            return envelope.Data;
        }

        throw ToException(response, envelope);
    }

    public static void EnsureSuccess<T>(IApiResponse<ApiEnvelope<T>> response)
    {
        if (response.IsSuccessStatusCode &&
            response.Content is not { Success: false })
        {
            return;
        }

        throw ToException(response, response.Content);
    }

    private static ApiServiceException ToException<T>(
        IApiResponse<ApiEnvelope<T>> response,
        ApiEnvelope<T>? envelope)
    {
        var error = ResolveError(response, envelope);

        return new ApiServiceException(
            response.StatusCode,
            error,
            ApiErrorMessages.ForUser(error, response.StatusCode));
    }

    private static ApiError? ResolveError<T>(
        IApiResponse<ApiEnvelope<T>> response,
        ApiEnvelope<T>? envelope)
    {
        if (envelope?.Error is { } envelopeError)
            return envelopeError;

        return TryParseErrorFromBody(response);
    }

    private static ApiError? TryParseErrorFromBody<T>(IApiResponse<ApiEnvelope<T>> response)
    {
        var json = response.Error?.Content;

        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ApiEnvelopeJson>(json, JsonOptions)?.Error;
        }
        catch
        {
            return null;
        }
    }

    private sealed record ApiEnvelopeJson(bool Success, JsonElement? Data, ApiError? Error);
}
