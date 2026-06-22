using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.DTOs.Comments;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Comments;

public record CreateBoardTaskCommentCommand(Guid BoardTaskId, string Body) : IRequest<Result<BoardTaskCommentDto>>;

public class CreateBoardTaskCommentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardTaskCommentRepository boardTaskCommentRepository,
    IUnitOfWork unitOfWork,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<CreateBoardTaskCommentCommand, Result<BoardTaskCommentDto>>
{
    public async Task<Result<BoardTaskCommentDto>> Handle(CreateBoardTaskCommentCommand request, CancellationToken ct)
    {
        var currentUserInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUserInfo is null)
            return Result<BoardTaskCommentDto>.Failure(GeneralErrors.Unauthorized);

        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanCommentOnTasks(authorizedRole))
            return Result<BoardTaskCommentDto>.Failure(GeneralErrors.Forbidden);

        var body = request.Body.Trim();

        var comment = new BoardTaskComment
        {
            BoardTaskId = request.BoardTaskId,
            AuthorId = currentUserInfo.Id,
            Body = body,
        };

        boardTaskCommentRepository.Add(comment);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<BoardTaskCommentDto>.Success(new BoardTaskCommentDto(
            comment.Id,
            comment.Body,
            comment.CreatedAtUtc,
            new UserDto(
                currentUserInfo.Id,
                currentUserInfo.Email,
                currentUserInfo.DisplayName,
                currentUserInfo.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(currentUserInfo.Id))));
    }
}

public class CreateBoardTaskCommentCommandValidator : AbstractValidator<CreateBoardTaskCommentCommand>
{
    public CreateBoardTaskCommentCommandValidator()
    {
        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x.Body)
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .WithMessage("'Body' must not be empty.")
            .MaximumLength(BoardTaskCommentFieldLengths.MaxBodyLength);
    }
}
