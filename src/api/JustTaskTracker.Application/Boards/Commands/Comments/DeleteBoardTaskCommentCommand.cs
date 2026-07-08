using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Comments;

public record DeleteBoardTaskCommentCommand(Guid BoardId, Guid CommentId)
    : IRequest<Result>, IRequireActiveBoard;

public class DeleteBoardTaskCommentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IUnitOfWork unitOfWork,
    IBoardActionNotifier boardActionNotifier,
    IDateTimeProvider dateTimeProvider)
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

        var boardTaskId = comment.BoardTaskId;

        boardTaskCommentRepository.Remove(comment);

        await unitOfWork.SaveChangesAsync(ct);

        var commentsCount = (await boardTaskCommentRepository.GetListByBoardTaskIdAsync(boardTaskId, ct)).Count;

        await boardActionNotifier.NotifyAsync(new BoardActionNotification(
            request.BoardId,
            BoardActionNotificationType.TaskCommentsCountChanged,
            currentUserInfo.Id,
            dateTimeProvider.UtcNow,
            new TaskCommentsCountChangedPayload(boardTaskId, commentsCount)), ct);

        return Result.Success();
    }
}

public class DeleteBoardTaskCommentCommandValidator : AbstractValidator<DeleteBoardTaskCommentCommand>
{
    public DeleteBoardTaskCommentCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.CommentId)
            .NotEmpty();
    }
}
