using JustTaskTracker.Archival.Functions.Contracts.DTOs;
using JustTaskTracker.Archival.Functions.Contracts.Enums;

namespace JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;

public interface IBoardExportStatusNotifyApiClient
{
    Task NotifyExportStatusChangedAsync(Guid boardId, BoardExportStatus status, CancellationToken ct = default);

    Task NotifyReExportStatusChangedAsync(
        Guid boardId,
        BoardExportStatus status,
        BoardExportOptions? exportOptions = null,
        CancellationToken ct = default);
}
