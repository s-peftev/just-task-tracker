using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Enums;
using Microsoft.Azure.Cosmos;

namespace JustTaskTracker.Infrastructure.Common.ExternalProviders.CosmosDb;

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
}
