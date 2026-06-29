using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Common.ExternalProviders;

public interface IBoardSerializationStatusService
{
    /// <summary>
    /// Creates or replaces the serialization status document for the given board.
    /// </summary>
    /// <remarks>
    /// Uses upsert semantics: one document per board, keyed by <paramref name="boardId"/>.
    /// </remarks>
    Task UpdateSerializationStatusAsync(
        Guid boardId,
        BoardSerializationStatus status,
        string? errorMessage = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the serialization status document for <paramref name="boardId"/>, or <see langword="null"/> when none exists.
    /// </summary>
    Task<BoardSerializationStatusInfo?> GetBoardSerializationStatusAsync(
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
