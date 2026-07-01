using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Contracts.DTOs;
using JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.Api;

public sealed class BoardExportDataApiClient(
    HttpClient httpClient,
    IOptions<BoardExportApiClientOptions> options,
    ILogger<BoardExportDataApiClient> logger) : IBoardExportDataApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<BoardExportDataDto> GetExportDataAsync(Guid boardId, BoardExportOptions exportOptions, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(exportOptions);

        var clientOptions = options.Value;

        using var request = new HttpRequestMessage(HttpMethod.Post, $"internal/boards/{boardId:D}/export-data")
        {
            Content = JsonContent.Create(exportOptions, options: JsonOptions),
        };

        request.Headers.TryAddWithoutValidation(clientOptions.ApiKeyHeaderName, clientOptions.ApiKey);

        using var response = await httpClient.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Board {boardId} was not found for export.");
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException($"Board {boardId} is not eligible for export.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            logger.LogError(
                "Export data request failed. BoardId={BoardId}, StatusCode={StatusCode}, Body={Body}",
                boardId,
                (int)response.StatusCode,
                body);

            response.EnsureSuccessStatusCode();
        }

        var data = await response.Content.ReadFromJsonAsync<BoardExportDataDto>(JsonOptions, ct)
            ?? throw new InvalidOperationException($"Export data response for board {boardId} was empty.");

        if (data.Board.Id != boardId)
        {
            throw new InvalidOperationException(
                $"Export data board id mismatch. Expected {boardId}, got {data.Board.Id}.");
        }

        return data;
    }
}
