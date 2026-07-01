using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Errors;
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
    IUnitOfWork unitOfWork,
    ILogger<ReExportArchivedBoardCommandHandler> logger)
    : IRequestHandler<ReExportArchivedBoardCommand, Result>
{
    public async Task<Result> Handle(ReExportArchivedBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanArchiveBoard(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        if (board is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (!board.IsArchived || !board.IsExported)
            return Result.Failure(BoardsErrors.ReExportNotAllowed);

        if (board.IsReExportRequested)
            return Result.Failure(BoardsErrors.ReExportAlreadyRequested);

        var exportInfo = await boardExportService.GetBoardExportInfoAsync(board.Id, ct);

        if (exportInfo is null)
            return Result.Failure(BoardsErrors.SerializationInfoNotFound);

        if (exportInfo.ExportStatus != BoardExportStatus.Completed)
            return Result.Failure(BoardsErrors.ExportNotCompleted);

        if (exportInfo.ExportOptions == request.ReExportOptions)
            return Result.Failure(BoardsErrors.ReExportOptionsUnchanged);

        try
        {
            await boardExportService.SetReExportAsync(
                board.Id,
                BoardExportStatus.Pending,
                request.ReExportOptions,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to schedule re-export for board {BoardId}.", board.Id);
            return Result.Failure(GeneralErrors.ServiceUnavailable);
        }

        board.IsReExportRequested = true;

        await unitOfWork.SaveChangesAsync(ct);

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
