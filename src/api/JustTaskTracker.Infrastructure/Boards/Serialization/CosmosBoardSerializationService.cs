using System.Net;
using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;
using Microsoft.Azure.Cosmos;

namespace JustTaskTracker.Infrastructure.Boards.Serialization;

internal sealed class CosmosBoardSerializationService(Container container, IDateTimeProvider dateTimeProvider) : IBoardSerializationService
{
    public async Task SetSerializationAsync(
        Guid boardId,
        BoardSerializationStatus status,
        BoardArchiveExportOptions exportOptions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(exportOptions);

        var document = new BoardSerializationDocument
        {
            Id = boardId.ToString(),
            BoardId = boardId,
            Status = (int)status,
            StatusName = status.ToString(),
            UpdatedAtUtc = dateTimeProvider.UtcNow,
            ErrorMessage = null,
            ExportOptions = exportOptions,
        };

        await container.UpsertItemAsync(
            document,
            new PartitionKey(boardId.ToString()),
            cancellationToken: ct);
    }

    public async Task SetReExportAsync(
        Guid boardId,
        BoardSerializationStatus reExportStatus,
        BoardArchiveExportOptions reExportOptions,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(reExportOptions);

        await container.PatchItemAsync<BoardSerializationDocument>(
            boardId.ToString(),
            new PartitionKey(boardId.ToString()),
            [
                PatchOperation.Set($"/{BoardSerializationDocument.ReExportStatusJson}", (int)reExportStatus),
                PatchOperation.Set($"/{BoardSerializationDocument.ReExportStatusNameJson}", reExportStatus.ToString()),
                PatchOperation.Set($"/{BoardSerializationDocument.ReExportOptionsJson}", reExportOptions),
            ],
            cancellationToken: ct);
    }

    public async Task UpdateStatusAsync(
        Guid boardId,
        BoardSerializationStatus status,
        string? errorMessage = null,
        CancellationToken ct = default)
    {
        await container.PatchItemAsync<BoardSerializationDocument>(
            boardId.ToString(),
            new PartitionKey(boardId.ToString()),
            [
                PatchOperation.Set($"/{BoardSerializationDocument.StatusJson}", (int)status),
                PatchOperation.Set($"/{BoardSerializationDocument.StatusNameJson}", status.ToString()),
                PatchOperation.Set($"/{BoardSerializationDocument.UpdatedAtUtcJson}", dateTimeProvider.UtcNow),
                PatchOperation.Set($"/{BoardSerializationDocument.ErrorMessageJson}", errorMessage),
            ],
            cancellationToken: ct);
    }

    public async Task<BoardSerializationStatusInfo?> GetBoardSerializationInfoAsync(
        Guid boardId,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);

        try
        {
            var response = await container.ReadItemAsync<BoardSerializationDocument>(
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

    public async Task<IReadOnlyDictionary<Guid, BoardSerializationStatusInfo>> GetBoardListSerializationStatusesAsync(
        IReadOnlyCollection<Guid> boardIds,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(boardIds);

        if (boardIds.Count == 0)
            return new Dictionary<Guid, BoardSerializationStatusInfo>();

        var itemIds = boardIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Select(id => (id.ToString(), new PartitionKey(id.ToString())))
            .ToList();

        if (itemIds.Count == 0)
            return new Dictionary<Guid, BoardSerializationStatusInfo>();

        var response = await container.ReadManyItemsAsync<BoardSerializationDocument>(
            itemIds,
            cancellationToken: ct);

        return response
            .Select(ToInfo)
            .ToDictionary(status => status.BoardId);
    }

    private static BoardSerializationStatusInfo ToInfo(BoardSerializationDocument document) =>
        new(
            document.BoardId,
            (BoardSerializationStatus)document.Status,
            document.UpdatedAtUtc,
            document.ErrorMessage,
            document.ExportOptions);
}
