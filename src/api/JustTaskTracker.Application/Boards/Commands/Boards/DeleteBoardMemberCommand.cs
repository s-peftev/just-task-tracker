using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record DeleteBoardMemberCommand(Guid BoardId, Guid UserId) : IRequest<Result>;

public class DeleteBoardMemberCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBoardMemberCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardMemberCommand request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageMembers(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        var member = await boardRepository.GetMemberAsync(request.BoardId, request.UserId, ct);

        if (member is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (member.Role == BoardMemberRole.Owner)
            return Result.Failure(BoardMembersErrors.OwnerCannotBeRemoved);

        boardRepository.RemoveMember(member);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeleteBoardMemberCommandValidator : AbstractValidator<DeleteBoardMemberCommand>
{
    public DeleteBoardMemberCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
