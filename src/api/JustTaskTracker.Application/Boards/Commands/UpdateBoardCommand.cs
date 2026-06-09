using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Boards.Authorization;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record UpdateBoardCommand(Guid BoardId, string Name) : IRequest<Result>;

public class UpdateBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBoardCommand, Result>
{
    public async Task<Result> Handle(UpdateBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (board is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (userRole is not { } role || !BoardRolePermissions.CanRenameBoard(role))
            return Result.Failure(GeneralErrors.Forbidden);

        var name = request.Name.Trim();

        if (string.Equals(board.Name, name, StringComparison.OrdinalIgnoreCase))
            return Result.Success();

        board.Name = name;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
