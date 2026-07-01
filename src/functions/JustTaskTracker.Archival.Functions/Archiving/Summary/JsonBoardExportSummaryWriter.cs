using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using JustTaskTracker.Archival.Functions.Abstractions.Archiving;
using JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

namespace JustTaskTracker.Archival.Functions.Archiving.Summary;

public sealed class JsonBoardExportSummaryWriter : IBoardExportSummaryWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public BoardExportSummaryFormat Format => BoardExportSummaryFormat.Json;

    public async Task WriteAsync(ZipArchive archive, BoardExportDataDto data, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(data);

        var entry = archive.CreateEntry(BoardArchiveEntryNames.SummaryJson, CompressionLevel.Optimal);

        await using var entryStream = entry.Open();
        await JsonSerializer.SerializeAsync(entryStream, data, JsonOptions, ct);
    }
}
