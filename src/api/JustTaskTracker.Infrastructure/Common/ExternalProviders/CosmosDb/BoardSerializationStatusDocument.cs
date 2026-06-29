using Newtonsoft.Json;

namespace JustTaskTracker.Infrastructure.Common.ExternalProviders.CosmosDb;

/// <summary>
/// Cosmos DB document that tracks the serialization/export status of an archived board.
/// Partition key: /boardId. One document per board — upsert replaces the previous entry.
/// </summary>
internal sealed class BoardSerializationStatusDocument
{
    /// <summary>
    /// Cosmos DB document identifier. Equals <see cref="BoardId"/> to enforce one document per board.
    /// </summary>
    [JsonProperty("id")]
    public required string Id { get; init; }

    [JsonProperty("boardId")]
    public required Guid BoardId { get; init; }

    [JsonProperty("status")]
    public required int Status { get; init; }

    [JsonProperty("statusName")]
    public required string StatusName { get; init; }

    [JsonProperty("updatedAtUtc")]
    public required DateTime UpdatedAtUtc { get; init; }

    [JsonProperty("errorMessage")]
    public string? ErrorMessage { get; init; }
}
