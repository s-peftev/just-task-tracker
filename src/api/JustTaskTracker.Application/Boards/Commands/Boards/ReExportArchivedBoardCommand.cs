using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Boards.Messaging;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record ReExportArchivedBoardCommand(Guid BoardId, BoardExportOptions ReExportOptions)
    : IRequest<Result>;

public class ReExportArchivedBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardExportService boardExportService,
    IBoardExportStatusNotifier exportStatusNotifier,
    ILogger<ReExportArchivedBoardCommandHandler> logger)
    : IRequestHandler<ReExportArchivedBoardCommand, Result>
{
    public async Task<Result> Handle(ReExportArchivedBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanExportBoard(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        if (board is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (!board.IsArchived)
            return Result.Failure(BoardsErrors.ReExportNotAllowed);

        var exportInfo = await boardExportService.GetBoardExportInfoAsync(board.Id, ct);

        if (exportInfo is null)
            return Result.Failure(BoardsErrors.ExportInfoNotFound);

        if (exportInfo.ExportStatus != BoardExportStatus.Completed)
            return Result.Failure(BoardsErrors.ReExportNotAllowed);

        if (exportInfo.ReExportStatus is BoardExportStatus.Requested
            or BoardExportStatus.Pending
            or BoardExportStatus.Processing)
            return Result.Failure(BoardsErrors.ReExportAlreadyRequested);

        if (exportInfo.ExportOptions == request.ReExportOptions)
            return Result.Failure(BoardsErrors.ReExportOptionsUnchanged);

        try
        {
            await boardExportService.SetReExportAsync(
                board.Id,
                BoardExportStatus.Requested,
                request.ReExportOptions,
                ct);

            await exportStatusNotifier.NotifyReExportStatusChangedAsync(
                new BoardExportStatusChangedNotification(board.Id, BoardExportStatus.Requested),
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to schedule re-export for board {BoardId}.", board.Id);
            return Result.Failure(GeneralErrors.ServiceUnavailable);
        }

        return Result.Success();
    }
}

public class ReExportArchivedBoardCommandValidator : AbstractValidator<ReExportArchivedBoardCommand>
{
    public ReExportArchivedBoardCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ReExportOptions)
            .NotNull();
    }
}
