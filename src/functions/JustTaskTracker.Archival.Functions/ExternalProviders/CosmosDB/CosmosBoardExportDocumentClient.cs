using JustTaskTracker.Archival.Functions.Abstractions.ExternalProviders;
using JustTaskTracker.Archival.Functions.Contracts.DTOs;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.CosmosDB;

public class CosmosBoardExportDocumentClient : IBoardExportDocumentClient
{
    public Task<BoardExportStatusInfo?> GetBoardExportInfoAsync(Guid boardId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task MarkExportProcessingAsync(Guid boardId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task CompleteInitialExportAsync(Guid boardId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task FailInitialExportAsync(Guid boardId, string errorMessage, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task MarkReExportProcessingAsync(Guid boardId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task CompleteReExportAsync(Guid boardId, BoardExportOptions promotedExportOptions, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task FailReExportAsync(Guid boardId, string errorMessage, CancellationToken ct = default) =>
        throw new NotImplementedException();
}
