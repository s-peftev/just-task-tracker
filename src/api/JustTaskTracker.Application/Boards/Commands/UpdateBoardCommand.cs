using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record UpdateBoardCommand(Guid BoardId, string Name) : IRequest<Result<BoardDetailsDto>>;

public class UpdateBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBoardCommand, Result<BoardDetailsDto>>
{
    public async Task<Result<BoardDetailsDto>> Handle(UpdateBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (board is null)
            return Result<BoardDetailsDto>.Failure(GeneralErrors.NotFound);

        if (userRole is not { } role || !BoardRolePermissions.CanRenameBoard(role))
            return Result<BoardDetailsDto>.Failure(GeneralErrors.Forbidden);

        board.Name = request.Name;

        await unitOfWork.SaveChangesAsync(ct);

        var details = await boardRepository.GetBoardDetailsByIdAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        return Result<BoardDetailsDto>.Success(details!);
    }
}
