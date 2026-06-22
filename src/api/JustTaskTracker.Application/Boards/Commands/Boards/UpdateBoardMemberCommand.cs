using FluentValidation;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record UpdateBoardMemberCommand(Guid BoardId, Guid UserId, BoardMemberRole Role) : IRequest<Result>;

public class UpdateBoardMemberCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBoardMemberCommand, Result>
{
    public async Task<Result> Handle(UpdateBoardMemberCommand request, CancellationToken ct)
    {
        if (request.Role == BoardMemberRole.Owner)
            return Result.Failure(BoardMembersErrors.OwnerRoleNotAllowed);

        var currentMember = await boardRepository.GetMemberByAzureAOIAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (currentMember is null || !BoardRolePermissions.CanManageMembers(currentMember.Role))
            return Result.Failure(GeneralErrors.Forbidden);

        if (currentMember.UserId == request.UserId)
            return Result.Failure(BoardMembersErrors.CannotChangeOwnRole);

        var member = await boardRepository.GetMemberAsync(request.BoardId, request.UserId, ct);

        if (member is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (member.Role == BoardMemberRole.Owner)
            return Result.Failure(BoardMembersErrors.OwnerRoleCannotBeChanged);

        if (member.Role == request.Role)
            return Result.Success();

        member.Role = request.Role;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class UpdateBoardMemberCommandValidator : AbstractValidator<UpdateBoardMemberCommand>
{
    public UpdateBoardMemberCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Role)
            .IsInEnum();
    }
}
