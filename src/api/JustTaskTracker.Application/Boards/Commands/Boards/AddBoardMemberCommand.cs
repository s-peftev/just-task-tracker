using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Enums;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record AddBoardMemberCommand(Guid BoardId, Guid UserId, BoardMemberRole Role)
    : IRequest<Result>, IRequireActiveBoard;

public class AddBoardMemberCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddBoardMemberCommand, Result>
{
    public async Task<Result> Handle(AddBoardMemberCommand request, CancellationToken ct)
    {
        if (request.Role == BoardMemberRole.Owner)
            return Result.Failure(BoardMembersErrors.OwnerRoleNotAllowed);

        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageMembers(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        if (await userRepository.GetByIdAsync(request.UserId, ct) is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (await boardRepository.IsBoardMemberAsync(request.BoardId, request.UserId, ct))
            return Result.Failure(BoardMembersErrors.UserAlreadyMember);

        boardRepository.AddMember(new BoardMember
        {
            BoardId = request.BoardId,
            UserId = request.UserId,
            Role = request.Role,
        });

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class AddBoardMemberCommandValidator : AbstractValidator<AddBoardMemberCommand>
{
    public AddBoardMemberCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Role)
            .IsInEnum();
    }
}
