using System.Text.Json;
using JustTaskTracker.Archival.Functions.Contracts.Api;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.Api;

/// <summary>
/// Unwraps the <see cref="ApiEnvelope{T}"/> returned by the API into the payload <typeparamref name="T"/>.
/// </summary>
internal static class ApiEnvelopeReader
{
    private const int MaxLoggedBodyLength = 2048;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task<T> ReadSuccessDataAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(response);

        var json = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("API response body was empty.");

        ApiEnvelope<T> envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<ApiEnvelope<T>>(json, JsonOptions)
                ?? throw new InvalidOperationException("API response body was empty.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"API response could not be deserialized. Body={Truncate(json)}",
                ex);
        }

        if (!envelope.Success)
            throw new InvalidOperationException(FormatFailureMessage(envelope.Error));

        if (envelope.Data is not { } data)
            throw new InvalidOperationException("API response data was empty.");

        return data;
    }

    private static string FormatFailureMessage(ApiError? error)
    {
        if (error is null)
            return "API request failed.";

        if (error.Details is { Count: > 0 } details)
            return $"API request failed ({error.Code}): {string.Join("; ", details)}";

        return $"API request failed ({error.Code}).";
    }

    private static string Truncate(string value) =>
        value.Length <= MaxLoggedBodyLength
            ? value
            : value[..MaxLoggedBodyLength] + "...";
}
