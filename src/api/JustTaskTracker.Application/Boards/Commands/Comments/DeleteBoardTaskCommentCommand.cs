using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Comments;

public record DeleteBoardTaskCommentCommand(Guid CommentId) : IRequest<Result>;

public class DeleteBoardTaskCommentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBoardTaskCommentCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardTaskCommentCommand request, CancellationToken ct)
    {
        var currentUserInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUserInfo is null)
            return Result.Failure(GeneralErrors.Unauthorized);

        var (comment, userRole) = await boardTaskCommentRepository.GetBoardTaskCommentWithUserRole(request.CommentId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanCommentOnTasks(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        if (comment is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (comment.AuthorId != currentUserInfo.Id)
            return Result.Failure(GeneralErrors.Forbidden);

        boardTaskCommentRepository.Remove(comment);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class DeleteBoardTaskCommentCommandValidator : AbstractValidator<DeleteBoardTaskCommentCommand>
{
    public DeleteBoardTaskCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty();
    }
}
