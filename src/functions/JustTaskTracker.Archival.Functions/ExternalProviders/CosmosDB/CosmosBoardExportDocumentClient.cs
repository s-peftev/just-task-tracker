using System.Net;
using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Contracts.DTOs;
using JustTaskTracker.Archival.Functions.Contracts.Enums;
using Microsoft.Azure.Cosmos;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.CosmosDB;

public sealed class CosmosBoardExportDocumentClient(Container container) : IBoardExportDocumentClient
{
    public async Task<BoardExportStatusInfo?> GetBoardExportInfoAsync(Guid boardId, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);

        try
        {
            var response = await container.ReadItemAsync<BoardExportDocument>(
                boardId.ToString(),
                ToPartitionKey(boardId),
                cancellationToken: ct);

            return ToInfo(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task MarkExportProcessingAsync(Guid boardId, CancellationToken ct = default) =>
        PatchExportStatusAsync(boardId, BoardExportStatus.Processing, errorMessage: null, ct);

    public Task CompleteInitialExportAsync(Guid boardId, CancellationToken ct = default) =>
        PatchExportStatusAsync(boardId, BoardExportStatus.Completed, errorMessage: null, ct);

    public Task FailInitialExportAsync(Guid boardId, string errorMessage, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return PatchExportStatusAsync(boardId, BoardExportStatus.Failed, errorMessage, ct);
    }

    public Task MarkReExportProcessingAsync(Guid boardId, CancellationToken ct = default) =>
        PatchReExportStatusAsync(boardId, BoardExportStatus.Processing, ct);

    public async Task CompleteReExportAsync(Guid boardId, BoardExportOptions promotedExportOptions, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(promotedExportOptions);

        var none = BoardExportStatus.None;

        await container.PatchItemAsync<BoardExportDocument>(
            boardId.ToString(),
            ToPartitionKey(boardId),
            [
                PatchOperation.Set($"/{BoardExportDocument.ExportOptionsJson}", promotedExportOptions),
                PatchOperation.Set($"/{BoardExportDocument.ExportStatusJson}", (int)BoardExportStatus.Completed),
                PatchOperation.Set($"/{BoardExportDocument.ExportStatusNameJson}", BoardExportStatus.Completed.ToString()),
                PatchOperation.Set($"/{BoardExportDocument.ReExportStatusJson}", (int)none),
                PatchOperation.Set($"/{BoardExportDocument.ReExportStatusNameJson}", none.ToString()),
                PatchOperation.Set<BoardExportOptions?>($"/{BoardExportDocument.ReExportOptionsJson}", null),
                PatchOperation.Set($"/{BoardExportDocument.UpdatedAtUtcJson}", DateTime.UtcNow),
                PatchOperation.Set<string?>($"/{BoardExportDocument.ErrorMessageJson}", null),
            ],
            cancellationToken: ct);
    }

    public Task FailReExportAsync(Guid boardId, string errorMessage, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return PatchReExportStatusAsync(boardId, BoardExportStatus.Failed, ct, errorMessage);
    }

    private async Task PatchExportStatusAsync(Guid boardId, BoardExportStatus status, string? errorMessage, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);

        await container.PatchItemAsync<BoardExportDocument>(
            boardId.ToString(),
            ToPartitionKey(boardId),
            [
                PatchOperation.Set($"/{BoardExportDocument.ExportStatusJson}", (int)status),
                PatchOperation.Set($"/{BoardExportDocument.ExportStatusNameJson}", status.ToString()),
                PatchOperation.Set($"/{BoardExportDocument.UpdatedAtUtcJson}", DateTime.UtcNow),
                PatchOperation.Set($"/{BoardExportDocument.ErrorMessageJson}", errorMessage),
            ],
            cancellationToken: ct);
    }

    private async Task PatchReExportStatusAsync(Guid boardId, BoardExportStatus status, CancellationToken ct, string? errorMessage = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);

        await container.PatchItemAsync<BoardExportDocument>(
            boardId.ToString(),
            ToPartitionKey(boardId),
            [
                PatchOperation.Set($"/{BoardExportDocument.ReExportStatusJson}", (int)status),
                PatchOperation.Set($"/{BoardExportDocument.ReExportStatusNameJson}", status.ToString()),
                PatchOperation.Set($"/{BoardExportDocument.UpdatedAtUtcJson}", DateTime.UtcNow),
                PatchOperation.Set($"/{BoardExportDocument.ErrorMessageJson}", errorMessage),
            ],
            cancellationToken: ct);
    }

    private static PartitionKey ToPartitionKey(Guid boardId) => new(boardId.ToString());

    private static BoardExportStatusInfo ToInfo(BoardExportDocument document) =>
        new(
            document.BoardId,
            document.UpdatedAtUtc,
            (BoardExportStatus)document.ExportStatus,
            document.ExportOptions,
            document.ReExportStatus is { } reExportStatus
                ? (BoardExportStatus)reExportStatus
                : BoardExportStatus.None,
            document.ReExportOptions,
            document.ErrorMessage);
}
