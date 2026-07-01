using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Contracts.DTOs;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.CosmosDB;

public class CosmosBoardExportDocumentClient : IBoardExportDocumentClient
{
    public async Task<BoardExportStatusInfo?> GetBoardExportInfoAsync(Guid boardId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
