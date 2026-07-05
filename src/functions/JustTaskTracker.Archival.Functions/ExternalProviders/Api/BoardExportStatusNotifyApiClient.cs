using System.Net.Http.Json;
using System.Text.Json;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Contracts.DTOs;
using JustTaskTracker.Archival.Functions.Contracts.Enums;
using JustTaskTracker.Archival.Functions.Contracts.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.Api;

public sealed class BoardExportStatusNotifyApiClient(
    HttpClient httpClient,
    IOptions<BoardExportApiClientOptions> options,
    ILogger<BoardExportStatusNotifyApiClient> logger) : IBoardExportStatusNotifyApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task NotifyExportStatusChangedAsync(
        Guid boardId,
        BoardExportStatus status,
        CancellationToken ct = default) =>
        NotifyAsync(boardId, "export-status-notify", status, exportOptions: null, ct);

    public Task NotifyReExportStatusChangedAsync(
        Guid boardId,
        BoardExportStatus status,
        BoardExportOptions? exportOptions = null,
        CancellationToken ct = default) =>
        NotifyAsync(boardId, "re-export-status-notify", status, exportOptions, ct);

    private async Task NotifyAsync(
        Guid boardId,
        string routeSuffix,
        BoardExportStatus status,
        BoardExportOptions? exportOptions,
        CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);

        var clientOptions = options.Value;
        var notification = new BoardExportStatusChangedNotification(boardId, status, exportOptions);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"internal/boards/{boardId:D}/{routeSuffix}")
            {
                Content = JsonContent.Create(notification, options: JsonOptions),
            };

            request.Headers.TryAddWithoutValidation(clientOptions.ApiKeyHeaderName, clientOptions.ApiKey);

            using var response = await httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);

                logger.LogWarning(
                    "Export status notify request failed. BoardId={BoardId}, Status={Status}, Route={Route}, StatusCode={StatusCode}, Body={Body}",
                    boardId,
                    status,
                    routeSuffix,
                    (int)response.StatusCode,
                    body);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "Export status notify request failed. BoardId={BoardId}, Status={Status}, Route={Route}",
                boardId,
                status,
                routeSuffix);
        }
    }
}
