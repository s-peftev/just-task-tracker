using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record CreateBoardTaskCommentCommand(Guid BoardId, Guid ColumnId, Guid BoardTaskId, string Body) : IRequest<Result<BoardTaskCommentDto>>;

public class CreateBoardTaskCommentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBoardTaskCommentCommand, Result<BoardTaskCommentDto>>
{
    public async Task<Result<BoardTaskCommentDto>> Handle(CreateBoardTaskCommentCommand request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanCommentOnTasks) is { } failure)
            return Result<BoardTaskCommentDto>.Failure(failure.Error);

        if (!await boardTaskRepository.ExistsByBoardIdAndColumnIdAndIdAsync(
                request.BoardId,
                request.ColumnId,
                request.BoardTaskId,
                ct))
            return Result<BoardTaskCommentDto>.Failure(GeneralErrors.NotFound);

        var currentUser = await userRepository.GetUserDtoByAzureAOIAsync(
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (currentUser is null)
            return Result<BoardTaskCommentDto>.Failure(GeneralErrors.Unauthorized);

        var body = request.Body.Trim();

        var comment = new BoardTaskComment
        {
            BoardTaskId = request.BoardTaskId,
            AuthorId = currentUser.Id,
            Body = body,
        };

        boardTaskCommentRepository.Add(comment);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<BoardTaskCommentDto>.Success(new BoardTaskCommentDto(
            comment.Id,
            comment.Body,
            comment.CreatedAtUtc,
            currentUser));
    }
}

public class CreateBoardTaskCommentCommandValidator : AbstractValidator<CreateBoardTaskCommentCommand>
{
    public CreateBoardTaskCommentCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x.Body)
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .WithMessage("'Body' must not be empty.")
            .MaximumLength(BoardTaskCommentFieldLengths.MaxBodyLength);
    }
}
