using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Boards;

public record UpdateBoardCommand(Guid BoardId, string Name)
    : IRequest<Result>, IRequireActiveBoard;

public class UpdateBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork,
    IBoardActionNotifier boardActionNotifier,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateBoardCommand, Result>
{
    public async Task<Result> Handle(UpdateBoardCommand request, CancellationToken ct)
    {
        var currentUserInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUserInfo is null)
            return Result.Failure(GeneralErrors.Unauthorized);

        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanRenameBoard(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        var name = request.Name.Trim();

        if (string.Equals(board!.Name, name, StringComparison.OrdinalIgnoreCase))
            return Result.Success();

        board.Name = name;

        await unitOfWork.SaveChangesAsync(ct);

        await boardActionNotifier.NotifyAsync(new BoardActionNotification(
            board.Id,
            BoardActionNotificationType.BoardRenamed,
            currentUserInfo.Id,
            dateTimeProvider.UtcNow,
            new BoardRenamedPayload(name)), ct);

        return Result.Success();
    }
}

public class UpdateBoardCommandValidator : AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("'Name' must not be empty.")
            .MaximumLength(BoardFieldLengths.MaxNameLength);
    }
}
