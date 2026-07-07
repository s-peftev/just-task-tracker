using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Mappings;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Columns;

public record ReorderColumnsCommand(Guid BoardId, Guid ColumnId, int Position)
    : IRequest<Result>, IRequireActiveBoard;

public class ReorderColumnsCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IColumnRepository columnRepository,
    IBoardPositioningService boardPositioningService,
    IUnitOfWork unitOfWork,
    IBoardActionNotifier boardActionNotifier,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<ReorderColumnsCommand, Result>
{
    public async Task<Result> Handle(ReorderColumnsCommand request, CancellationToken ct)
    {
        var currentUserInfo = await userRepository.GetUserInfoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUserInfo is null)
            return Result.Failure(GeneralErrors.Unauthorized);

        var userRole = await columnRepository.GetUserRoleAsync(request.ColumnId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageColumns(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        var columns = await columnRepository.GetListByBoardIdAsync(request.BoardId, ct);

        if (!columns.Any(column => column.Id == request.ColumnId))
            return Result.Failure(GeneralErrors.NotFound);

        if (request.Position < 0 || request.Position >= columns.Count)
            return Result.Failure(ColumnsErrors.InvalidPosition);

        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            await boardPositioningService.MoveToIndexAndSaveAsync(columns, request.ColumnId, request.Position, ct);
            await unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        var reorderedColumns = await columnRepository.GetListByBoardIdAsync(request.BoardId, ct);

        await boardActionNotifier.NotifyAsync(new BoardActionNotification(
            request.BoardId,
            BoardActionNotificationType.ColumnsReordered,
            currentUserInfo.Id,
            dateTimeProvider.UtcNow,
            new ColumnsReorderedPayload(BoardActionPositionMappings.ToColumnPositions(reorderedColumns))), ct);

        return Result.Success();
    }
}

public class ReorderColumnsCommandValidator : AbstractValidator<ReorderColumnsCommand>
{
    public ReorderColumnsCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0);
    }
}
