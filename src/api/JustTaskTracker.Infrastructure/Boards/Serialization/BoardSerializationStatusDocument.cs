using JustTaskTracker.Domain.Boards.DTOs.Boards;
using Newtonsoft.Json;

namespace JustTaskTracker.Infrastructure.Boards.Serialization;

/// <summary>
/// Cosmos DB document that tracks the serialization/export status of an archived board.
/// Partition key: /boardId. One document per board — upsert replaces the previous entry.
/// </summary>
internal sealed class BoardSerializationStatusDocument
{
    internal const string IdJson = "id";
    internal const string BoardIdJson = "boardId";
    internal const string StatusJson = "status";
    internal const string StatusNameJson = "statusName";
    internal const string UpdatedAtUtcJson = "updatedAtUtc";
    internal const string ErrorMessageJson = "errorMessage";
    internal const string ExportOptionsJson = "exportOptions";

    /// <summary>
    /// Cosmos DB document identifier. Equals <see cref="BoardId"/> to enforce one document per board.
    /// </summary>
    [JsonProperty(IdJson)]
    public required string Id { get; init; }

    [JsonProperty(BoardIdJson)]
    public required Guid BoardId { get; init; }

    [JsonProperty(StatusJson)]
    public required int Status { get; init; }

    [JsonProperty(StatusNameJson)]
    public required string StatusName { get; init; }

    [JsonProperty(UpdatedAtUtcJson)]
    public required DateTime UpdatedAtUtc { get; init; }

    [JsonProperty(ErrorMessageJson)]
    public string? ErrorMessage { get; init; }

    [JsonProperty(ExportOptionsJson)]
    public BoardArchiveExportOptions? ExportOptions { get; init; }
}
