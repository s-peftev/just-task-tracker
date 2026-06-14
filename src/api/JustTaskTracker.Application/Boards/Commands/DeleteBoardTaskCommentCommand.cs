using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record DeleteBoardTaskCommentCommand(Guid BoardId, Guid ColumnId, Guid BoardTaskId, Guid CommentId) : IRequest<Result>;

public class DeleteBoardTaskCommentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBoardTaskCommentCommand, Result>
{
    public async Task<Result> Handle(DeleteBoardTaskCommentCommand request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanCommentOnTasks) is { } failure)
            return failure;

        if (!await boardTaskRepository.ExistsByBoardIdAndColumnIdAndIdAsync(
                request.BoardId,
                request.ColumnId,
                request.BoardTaskId,
                ct))
            return Result.Failure(GeneralErrors.NotFound);

        var currentUser = await userRepository.GetUserDtoByAzureAOIAsync(
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (currentUser is null)
            return Result.Failure(GeneralErrors.Unauthorized);

        var comment = await boardTaskCommentRepository.GetByBoardIdAndColumnIdAndTaskIdAndIdAsync(
            request.BoardId,
            request.ColumnId,
            request.BoardTaskId,
            request.CommentId,
            ct);

        if (comment is null)
            return Result.Failure(GeneralErrors.NotFound);

        if (comment.AuthorId != currentUser.Id)
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
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x.CommentId)
            .NotEmpty();
    }
}
