using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Common.ExternalProviders;

public interface IBoardSerializationStatusService
{
    /// <summary>
    /// Creates or replaces the serialization document for the given board, including export options.
    /// </summary>
    /// <remarks>
    /// Uses upsert semantics: one document per board, keyed by <paramref name="boardId"/>.
    /// </remarks>
    Task SetSerializationAsync(
        Guid boardId,
        BoardSerializationStatus status,
        BoardArchiveExportOptions exportOptions,
        CancellationToken ct = default);

    /// <summary>
    /// Updates only serialization status fields on an existing document.
    /// </summary>
    /// <remarks>
    /// Uses patch semantics and does not modify <c>exportOptions</c>.
    /// </remarks>
    Task UpdateStatusAsync(
        Guid boardId,
        BoardSerializationStatus status,
        string? errorMessage = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the serialization info document for <paramref name="boardId"/>, or <see langword="null"/> when none exists.
    /// </summary>
    Task<BoardSerializationStatusInfo?> GetBoardSerializationInfoAsync(
        Guid boardId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns serialization status documents for the given boards.
    /// </summary>
    /// <remarks>
    /// Boards without a document are omitted from the result.
    /// </remarks>
    Task<IReadOnlyDictionary<Guid, BoardSerializationStatusInfo>> GetBoardListSerializationStatusesAsync(
        IReadOnlyCollection<Guid> boardIds,
        CancellationToken ct = default);
}
