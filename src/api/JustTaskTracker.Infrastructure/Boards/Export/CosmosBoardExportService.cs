using System.Net;
using System.Runtime.CompilerServices;
using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;
using Microsoft.Azure.Cosmos;

namespace JustTaskTracker.Infrastructure.Boards.Export;

internal sealed class CosmosBoardExportService(Container container, IDateTimeProvider dateTimeProvider) : IBoardExportService
{
    public async Task SetExportAsync(Guid boardId, BoardExportStatus exportStatus, BoardExportOptions exportOptions, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(exportOptions);

        var document = new BoardExportDocument
        {
            Id = boardId.ToString(),
            BoardId = boardId,
            ExportStatus = (int)exportStatus,
            ExportStatusName = exportStatus.ToString(),
            UpdatedAtUtc = dateTimeProvider.UtcNow,
            ErrorMessage = null,
            ExportOptions = exportOptions,
        };

        await container.UpsertItemAsync(
            document,
            new PartitionKey(boardId.ToString()),
            cancellationToken: ct);
    }

    public async Task SetReExportAsync(Guid boardId, BoardExportStatus reExportStatus, BoardExportOptions reExportOptions, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reExportOptions);

        await container.PatchItemAsync<BoardExportDocument>(
            boardId.ToString(),
            new PartitionKey(boardId.ToString()),
            [
                PatchOperation.Set($"/{BoardExportDocument.ReExportStatusJson}", (int)reExportStatus),
                PatchOperation.Set($"/{BoardExportDocument.ReExportStatusNameJson}", reExportStatus.ToString()),
                PatchOperation.Set($"/{BoardExportDocument.ReExportOptionsJson}", reExportOptions),
            ],
            cancellationToken: ct);
    }

    public async Task UpdateExportStatusAsync(Guid boardId, BoardExportStatus status, string? errorMessage = null, CancellationToken ct = default)
    {
        await container.PatchItemAsync<BoardExportDocument>(
            boardId.ToString(),
            new PartitionKey(boardId.ToString()),
            [
                PatchOperation.Set($"/{BoardExportDocument.ExportStatusJson}", (int)status),
                PatchOperation.Set($"/{BoardExportDocument.ExportStatusNameJson}", status.ToString()),
                PatchOperation.Set($"/{BoardExportDocument.UpdatedAtUtcJson}", dateTimeProvider.UtcNow),
                PatchOperation.Set($"/{BoardExportDocument.ErrorMessageJson}", errorMessage),
            ],
            cancellationToken: ct);
    }

    public async Task<BoardExportStatusInfo?> GetBoardExportInfoAsync(Guid boardId, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);

        try
        {
            var response = await container.ReadItemAsync<BoardExportDocument>(
                boardId.ToString(),
                new PartitionKey(boardId.ToString()),
                cancellationToken: ct);

            return ToInfo(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyDictionary<Guid, BoardExportStatusInfo>> GetBoardListExportInfoAsync(IReadOnlyCollection<Guid> boardIds, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(boardIds);

        if (boardIds.Count == 0)
            return new Dictionary<Guid, BoardExportStatusInfo>();

        var itemIds = boardIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Select(id => (id.ToString(), new PartitionKey(id.ToString())))
            .ToList();

        if (itemIds.Count == 0)
            return new Dictionary<Guid, BoardExportStatusInfo>();

        var response = await container.ReadManyItemsAsync<BoardExportDocument>(
            itemIds,
            cancellationToken: ct);

        return response
            .Select(ToInfo)
            .ToDictionary(status => status.BoardId);
    }

    public async IAsyncEnumerable<BoardExportStatusInfo> ScanActionableAsync(int maxDocuments, DateTime failedCooldownThreshold, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE " +
            "c.exportStatus = @requested " +
            "OR (c.exportStatus = @failed AND c.updatedAtUtc <= @cooldown) " +
            "OR c.reExportStatus = @requested " +
            "OR (c.reExportStatus = @failed AND c.updatedAtUtc <= @cooldown)")
            .WithParameter("@requested", (int)BoardExportStatus.Requested)
            .WithParameter("@failed", (int)BoardExportStatus.Failed)
            .WithParameter("@cooldown", failedCooldownThreshold);

        using var iterator = container.GetItemQueryIterator<BoardExportDocument>(
            query,
            requestOptions: new QueryRequestOptions { MaxItemCount = maxDocuments });

        var fetched = 0;
        while (iterator.HasMoreResults && fetched < maxDocuments)
        {
            ct.ThrowIfCancellationRequested();
            var page = await iterator.ReadNextAsync(ct);
            foreach (var document in page)
            {
                yield return ToInfo(document);
                if (++fetched >= maxDocuments)
                    yield break;
            }
        }
    }

    public async Task UpdateReExportStatusAsync(Guid boardId, BoardExportStatus reExportStatus, string? errorMessage = null, CancellationToken ct = default)
    {
        await container.PatchItemAsync<BoardExportDocument>(
            boardId.ToString(),
            new PartitionKey(boardId.ToString()),
            [
                PatchOperation.Set($"/{BoardExportDocument.ReExportStatusJson}", (int)reExportStatus),
                PatchOperation.Set($"/{BoardExportDocument.ReExportStatusNameJson}", reExportStatus.ToString()),
                PatchOperation.Set($"/{BoardExportDocument.UpdatedAtUtcJson}", dateTimeProvider.UtcNow),
                PatchOperation.Set($"/{BoardExportDocument.ErrorMessageJson}", errorMessage),
            ],
            cancellationToken: ct);
    }

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
