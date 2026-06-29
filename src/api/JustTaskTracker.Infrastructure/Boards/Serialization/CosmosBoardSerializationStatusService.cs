using System.Net;
using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Enums;
using Microsoft.Azure.Cosmos;

namespace JustTaskTracker.Infrastructure.Boards.Serialization;

internal sealed class CosmosBoardSerializationStatusService(Container container, IDateTimeProvider dateTimeProvider) : IBoardSerializationStatusService
{
    public async Task UpdateSerializationStatusAsync(
        Guid boardId,
        BoardSerializationStatus status,
        string? errorMessage = null,
        CancellationToken ct = default)
    {
        var document = new BoardSerializationStatusDocument
        {
            Id = boardId.ToString(),
            BoardId = boardId,
            Status = (int)status,
            StatusName = status.ToString(),
            UpdatedAtUtc = dateTimeProvider.UtcNow,
            ErrorMessage = errorMessage,
        };

        await container.UpsertItemAsync(
            document,
            new PartitionKey(boardId.ToString()),
            cancellationToken: ct);
    }

    public async Task<BoardSerializationStatusInfo?> GetBoardSerializationStatusAsync(
        Guid boardId,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(boardId, Guid.Empty);

        try
        {
            var response = await container.ReadItemAsync<BoardSerializationStatusDocument>(
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

        var response = await container.ReadManyItemsAsync<BoardSerializationStatusDocument>(
            itemIds,
            cancellationToken: ct);

        return response
            .Select(ToInfo)
            .ToDictionary(status => status.BoardId);
    }

    private static BoardSerializationStatusInfo ToInfo(BoardSerializationStatusDocument document) =>
        new(
            document.BoardId,
            (BoardSerializationStatus)document.Status,
            document.UpdatedAtUtc,
            document.ErrorMessage);
}
