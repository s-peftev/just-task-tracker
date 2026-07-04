using System.IO.Compression;
using System.Text;
using JustTaskTracker.Archival.Functions.Abstractions.Archiving;
using JustTaskTracker.Archival.Functions.Archiving.Summary;
using JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Archival.Functions.Archiving;

public sealed class BoardArchiveBuilder(
    BoardExportSummaryWriterRegistry summaryWriterRegistry,
    IExportAttachmentFetcher attachmentFetcher,
    ILogger<BoardArchiveBuilder> logger) : IBoardArchiveBuilder
{
    public async Task<BoardExportArchive> BuildAsync(
        BoardExportDataDto data,
        IReadOnlyList<BoardExportSummaryFormat> summaryFormats,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(summaryFormats);

        if (summaryFormats.Count == 0)
        {
            throw new ArgumentException("At least one summary format is required.", nameof(summaryFormats));
        }

        var boardId = data.Board.Id;
        var fileName = $"{boardId:D}.zip";

        var resultStream = new MemoryStream();

        using (var archive = new ZipArchive(resultStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var format in summaryFormats)
            {
                var summaryWriter = summaryWriterRegistry.Get(format);
                await summaryWriter.WriteAsync(archive, data, ct);
            }

            if (data.AppliedOptions.IncludeAttachments)
            {
                await WriteAttachmentsAsync(archive, data, ct);
            }
        }

        resultStream.Position = 0;

        logger.LogInformation(
            "Board export archive built. BoardId={BoardId}, FileName={FileName}, SizeBytes={SizeBytes}",
            boardId,
            fileName,
            resultStream.Length);

        return new BoardExportArchive(resultStream, fileName);
    }

    private async Task WriteAttachmentsAsync(ZipArchive archive, BoardExportDataDto data, CancellationToken ct)
    {
        foreach (var column in data.Columns)
        {
            foreach (var task in column.Tasks)
            {
                if (task.Attachments is not { Count: > 0 })
                    continue;

                foreach (var attachment in task.Attachments.OrderBy(a => a.Position))
                {
                    ct.ThrowIfCancellationRequested();

                    var entryPath = BuildAttachmentEntryPath(task.Id, attachment);
                    var entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);

                    try
                    {
                        await using var attachmentStream = await attachmentFetcher.DownloadAsync(attachment, ct);
                        await using var entryStream = entry.Open();
                        await attachmentStream.CopyToAsync(entryStream, ct);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Failed to download attachment {AttachmentId} ('{FileName}') for task {TaskId}. BoardId={BoardId}",
                            attachment.Id,
                            attachment.OriginalFileName,
                            task.Id,
                            data.Board.Id);

                        throw;
                    }
                }
            }
        }
    }

    private static string BuildAttachmentEntryPath(Guid taskId, BoardExportAttachmentDto attachment)
    {
        var safeName = SanitizeFileName(attachment.OriginalFileName);
        return $"{BoardArchiveEntryNames.AttachmentsFolder}/{taskId:D}/{attachment.Position:000}_{safeName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);

        foreach (var ch in name)
            sb.Append(invalidChars.Contains(ch) ? '_' : ch);

        return sb.Length > 0 ? sb.ToString() : "attachment";
    }
}
