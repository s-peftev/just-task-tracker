using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record ArchiveBoardCommand(Guid BoardId)
    : IRequest<Result<BoardArchivedDto>>, IRequireActiveBoard;

public class ArchiveBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardSerializationStatusService boardSerializationStatusService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ILogger<ArchiveBoardCommandHandler> logger)
    : IRequestHandler<ArchiveBoardCommand, Result<BoardArchivedDto>>
{
    public async Task<Result<BoardArchivedDto>> Handle(ArchiveBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanArchiveBoard(authorizedRole))
            return Result<BoardArchivedDto>.Failure(GeneralErrors.Forbidden);

        if (board is null)
            return Result<BoardArchivedDto>.Failure(GeneralErrors.NotFound);

        try
        {
            await boardSerializationStatusService.UpdateSerializationStatusAsync(
                board.Id,
                BoardSerializationStatus.Pending,
                ct: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update serialization status for board {BoardId}.", board.Id);
            return Result<BoardArchivedDto>.Failure(GeneralErrors.ServiceUnavailable);
        }

        var archivedAtUtc = dateTimeProvider.UtcNow;

        board.IsArchived = true;
        board.ArchivedAtUtc = archivedAtUtc;

        await unitOfWork.SaveChangesAsync(ct);

        return Result<BoardArchivedDto>.Success(new BoardArchivedDto(archivedAtUtc, BoardSerializationStatus.Pending));
    }
}

public class ArchiveBoardCommandValidator : AbstractValidator<ArchiveBoardCommand>
{
    public ArchiveBoardCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();
    }
}
