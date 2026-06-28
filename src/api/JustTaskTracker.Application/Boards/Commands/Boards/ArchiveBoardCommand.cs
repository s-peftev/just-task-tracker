using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record ArchiveBoardCommand(Guid BoardId)
    : IRequest<Result<BoardArchivedDto>>, IRequireActiveBoard;

public class ArchiveBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
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

        var archivedAtUtc = dateTimeProvider.UtcNow;

        board.IsArchived = true;
        board.ArchivedAtUtc = archivedAtUtc;

        await unitOfWork.SaveChangesAsync(ct);

        return Result<BoardArchivedDto>.Success(new BoardArchivedDto(archivedAtUtc));
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
