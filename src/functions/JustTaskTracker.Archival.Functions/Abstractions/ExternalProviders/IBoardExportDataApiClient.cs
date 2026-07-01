using JustTaskTracker.Archival.Functions.Contracts.DTOs;
using JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

namespace JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;

public interface IBoardExportDataApiClient
{
    Task<BoardExportDataDto> GetExportDataAsync(
        Guid boardId,
        BoardExportOptions options,
        CancellationToken ct = default);
}
