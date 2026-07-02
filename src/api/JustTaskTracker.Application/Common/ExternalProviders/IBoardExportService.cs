using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Common.ExternalProviders;

public interface IBoardExportService
{
    /// <summary>
    /// Streams Cosmos DB documents whose export or re-export status is
    /// <see cref="BoardExportStatus.Requested"/> or <see cref="BoardExportStatus.Failed"/>
    /// (failed only when <paramref name="failedCooldownThreshold"/> has elapsed).
    /// </summary>
    /// <remarks>
    /// At most <paramref name="maxDocuments"/> documents are yielded per call.
    /// </remarks>
    IAsyncEnumerable<BoardExportStatusInfo> ScanActionableAsync(int maxDocuments, DateTime failedCooldownThreshold, CancellationToken ct = default);

    /// <summary>
    /// Updates only re-export status fields on an existing export document.
    /// </summary>
    /// <remarks>
    /// Uses patch semantics and does not modify <c>reExportOptions</c>.
    /// </remarks>
    Task UpdateReExportStatusAsync(Guid boardId, BoardExportStatus reExportStatus, string? errorMessage = null, CancellationToken ct = default);
    /// <summary>
    /// Creates or replaces the export document for the given board, including export options.
    /// </summary>
    /// <remarks>
    /// Uses upsert semantics: one document per board, keyed by <paramref name="boardId"/>.
    /// </remarks>
    Task SetExportAsync(Guid boardId, BoardExportStatus exportStatus, BoardExportOptions exportOptions, CancellationToken ct = default);

    /// <summary>
    /// Updates only export status fields on an existing document.
    /// </summary>
    /// <remarks>
    /// Uses patch semantics and does not modify <c>exportOptions</c>.
    /// </remarks>
    Task UpdateExportStatusAsync(Guid boardId, BoardExportStatus status, string? errorMessage = null, CancellationToken ct = default);

    /// <summary>
    /// Updates only re-export fields on an existing export document.
    /// </summary>
    Task SetReExportAsync(Guid boardId, BoardExportStatus reExportStatus, BoardExportOptions reExportOptions, CancellationToken ct = default);

    /// <summary>
    /// Returns the export info document for <paramref name="boardId"/>, or <see langword="null"/> when none exists.
    /// </summary>
    Task<BoardExportStatusInfo?> GetBoardExportInfoAsync(Guid boardId, CancellationToken ct = default);

    /// <summary>
    /// Returns export info documents for the given boards.
    /// </summary>
    /// <remarks>
    /// Boards without a document are omitted from the result.
    /// </remarks>
    Task<IReadOnlyDictionary<Guid, BoardExportStatusInfo>> GetBoardListExportInfoAsync(IReadOnlyCollection<Guid> boardIds, CancellationToken ct = default);
}
