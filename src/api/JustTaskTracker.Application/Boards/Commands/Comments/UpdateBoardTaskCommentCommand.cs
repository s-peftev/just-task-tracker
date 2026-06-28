using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Comments;

public record UpdateBoardTaskCommentCommand(Guid BoardId, Guid CommentId, string Body)
    : IRequest<Result>, IRequireActiveBoard;

public class UpdateBoardTaskCommentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBoardTaskCommentCommand, Result>
{
    public async Task<Result> Handle(UpdateBoardTaskCommentCommand request, CancellationToken ct)
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

        var body = request.Body.Trim();

        if (string.Equals(comment.Body, body, StringComparison.Ordinal))
            return Result.Success();

        comment.Body = body;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class UpdateBoardTaskCommentCommandValidator : AbstractValidator<UpdateBoardTaskCommentCommand>
{
    public UpdateBoardTaskCommentCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.CommentId)
            .NotEmpty();

        RuleFor(x => x.Body)
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .WithMessage("'Body' must not be empty.")
            .MaximumLength(BoardTaskCommentFieldLengths.MaxBodyLength);
    }
}
