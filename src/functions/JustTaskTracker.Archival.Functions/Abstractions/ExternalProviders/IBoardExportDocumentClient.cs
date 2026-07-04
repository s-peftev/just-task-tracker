using JustTaskTracker.Archival.Functions.Contracts.DTOs;

namespace JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;

public interface IBoardExportDocumentClient
{
    Task<BoardExportStatusInfo?> GetBoardExportInfoAsync(Guid boardId, CancellationToken ct = default);

    Task MarkExportProcessingAsync(Guid boardId, CancellationToken ct = default);

    Task CompleteInitialExportAsync(Guid boardId, CancellationToken ct = default);

    Task FailInitialExportAsync(Guid boardId, string errorMessage, CancellationToken ct = default);

    Task MarkReExportProcessingAsync(Guid boardId, CancellationToken ct = default);

    Task CompleteReExportAsync(Guid boardId, BoardExportOptions promotedExportOptions, CancellationToken ct = default);

    Task FailReExportAsync(Guid boardId, string errorMessage, CancellationToken ct = default);
}
