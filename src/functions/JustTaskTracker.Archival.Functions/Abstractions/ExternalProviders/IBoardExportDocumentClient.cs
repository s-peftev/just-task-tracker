using JustTaskTracker.Archival.Functions.Contracts.DTOs;

namespace JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;

public interface IBoardExportDocumentClient
{
    Task<BoardExportStatusInfo?> GetAsync(Guid boardId, CancellationToken ct = default);
}
