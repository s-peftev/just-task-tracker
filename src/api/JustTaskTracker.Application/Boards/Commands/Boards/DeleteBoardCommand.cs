using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record DeleteBoardCommand(Guid BoardId) : IRequest<Result>;

public class DeleteBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBoardCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanDeleteBoard(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        boardRepository.Remove(board!);

        var boardColumns = await columnRepository.GetListByBoardIdAsync(request.BoardId, ct);
        columnRepository.RemoveRange(boardColumns);

        var boardTasks = await boardTaskRepository.GetListByBoardIdAsync(request.BoardId, ct);
        boardTaskRepository.RemoveRange(boardTasks);

        var boardComments = await boardTaskCommentRepository.GetListByBoardIdAsync(request.BoardId, ct);
        boardTaskCommentRepository.RemoveRange(boardComments);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
