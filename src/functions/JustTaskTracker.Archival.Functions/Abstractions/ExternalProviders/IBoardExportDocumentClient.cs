using JustTaskTracker.Archival.Functions.Contracts.DTOs;

namespace JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;

public interface IBoardExportDocumentClient
{
    Task<BoardExportStatusInfo?> GetBoardExportInfoAsync(Guid boardId, CancellationToken ct = default);
}
