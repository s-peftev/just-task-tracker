using FluentValidation;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record LeaveBoardCommand(Guid BoardId) : IRequest<Result>;

public class LeaveBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LeaveBoardCommand, Result>
{
    public async Task<Result> Handle(LeaveBoardCommand request, CancellationToken ct)
    {
        var currentMember = await boardRepository.GetMemberByAzureAOIAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (currentMember is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (!BoardRolePermissions.CanLeaveBoard(currentMember.Role))
            return Result.Failure(BoardMembersErrors.OwnerCannotLeaveBoard);

        boardRepository.RemoveMember(currentMember);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class LeaveBoardCommandValidator : AbstractValidator<LeaveBoardCommand>
{
    public LeaveBoardCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();
    }
}
