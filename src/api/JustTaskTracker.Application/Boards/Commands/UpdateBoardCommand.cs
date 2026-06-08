using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Enums;
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

        if (userRole != BoardMemberRole.Owner)
            return Result<BoardDetailsDto>.Failure(GeneralErrors.Unauthorized);

        board.Name = request.Name;

        await unitOfWork.SaveChangesAsync(ct);

        var details = await boardRepository.GetBoardDetailsByIdAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        return Result<BoardDetailsDto>.Success(details!);
    }
}
