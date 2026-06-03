using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Kanban.Repositories;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Kanban.Enums;
using MediatR;

namespace JustTaskTracker.Application.Kanban.Commands;

public record DeleteBoardCommand(Guid BoardId) : IRequest<Result>;

public class DeleteBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBoardCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (board is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (userRole != BoardMemberRole.Owner)
            return Result.Failure(GeneralErrors.Unauthorized);

        await boardRepository.RemoveByIdAsync(request.BoardId, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
