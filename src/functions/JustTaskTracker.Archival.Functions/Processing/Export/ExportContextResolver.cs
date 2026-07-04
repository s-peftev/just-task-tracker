using JustTaskTracker.Archival.Functions.Contracts.DTOs;
using JustTaskTracker.Archival.Functions.Contracts.Enums;
using JustTaskTracker.Archival.Functions.Contracts.Messaging;

namespace JustTaskTracker.Archival.Functions.Processing.Export;

public class ExportContextResolver
{
    public BoardExportContext Resolve(BoardExportMessage message, BoardExportStatusInfo info)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(info);

        if (message.BoardId != info.BoardId)
        {
            throw new InvalidOperationException(
                $"Board export document mismatch. Message board id: {message.BoardId}, document board id: {info.BoardId}.");
        }

        return message.Type switch
        {
            BoardExportType.InitialExport => ResolveInitialExport(message, info),
            BoardExportType.ReExport => ResolveReExport(message, info),
            _ => throw new ArgumentOutOfRangeException(nameof(message), message.Type, "Unsupported export type."),
        };
    }

    private static BoardExportContext ResolveInitialExport(BoardExportMessage message, BoardExportStatusInfo info)
    {
        if (!IsProcessable(info.ExportStatus))
        {
            return Skip(
                message.BoardId,
                message.Type,
                $"Export is not pending or requested (current status: {info.ExportStatus}).");
        }

        if (info.ExportOptions is null)
        {
            throw new InvalidOperationException(
                $"Export options are missing for board {message.BoardId}.");
        }

        return new BoardExportContext(message.BoardId, message.Type, info.ExportOptions);
    }

    private static BoardExportContext ResolveReExport(BoardExportMessage message, BoardExportStatusInfo info)
    {
        if (!IsProcessable(info.ReExportStatus))
        {
            return Skip(
                message.BoardId,
                message.Type,
                $"Re-export is not pending or requested (current status: {info.ReExportStatus}).");
        }

        if (info.ReExportOptions is null)
        {
            throw new InvalidOperationException(
                $"Re-export options are missing for board {message.BoardId}.");
        }

        return new BoardExportContext(message.BoardId, message.Type, info.ReExportOptions);
    }

    private static bool IsProcessable(BoardExportStatus status) =>
        status is BoardExportStatus.Pending or BoardExportStatus.Requested;

    private static BoardExportContext Skip(Guid boardId, BoardExportType type, string reason) =>
        new(boardId, type, Options: null, ShouldSkip: true, SkipReason: reason);
}
