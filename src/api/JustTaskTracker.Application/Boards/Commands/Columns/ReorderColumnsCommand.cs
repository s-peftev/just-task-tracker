using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Columns;

public record ReorderColumnsCommand(Guid BoardId, Guid ColumnId, int Position)
    : IRequest<Result>, IRequireActiveBoard;

public class ReorderColumnsCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IColumnRepository columnRepository,
    IBoardPositioningService boardPositioningService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReorderColumnsCommand, Result>
{
    public async Task<Result> Handle(ReorderColumnsCommand request, CancellationToken ct)
    {
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
