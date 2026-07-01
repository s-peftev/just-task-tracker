using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Contracts.DTOs;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.CosmosDB;

public class CosmosBoardExportDocumentClient : IBoardExportDocumentClient
{
    public Task<BoardExportStatusInfo?> GetAsync(Guid boardId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
