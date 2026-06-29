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
}
