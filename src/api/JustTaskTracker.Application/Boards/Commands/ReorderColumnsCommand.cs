using FluentValidation;
using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record ReorderColumnsCommand(Guid BoardId, IReadOnlyList<Guid> ColumnIds) : IRequest<Result>;

public class ReorderColumnsCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReorderColumnsCommand, Result>
{
    public async Task<Result> Handle(ReorderColumnsCommand request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanManageColumns) is { } failure)
            return failure;

        var columns = await columnRepository.GetListByBoardIdAsync(request.BoardId, ct);

        if (columns.Count != request.ColumnIds.Count)
            return Result.Failure(ColumnsErrors.InvalidOrder);

        var columnsById = columns.ToDictionary(c => c.Id);

        foreach (var columnId in request.ColumnIds)
        {
            if (!columnsById.ContainsKey(columnId))
                return Result.Failure(ColumnsErrors.InvalidOrder);
        }

        if (request.ColumnIds.SequenceEqual(columns.Select(c => c.Id)))
            return Result.Success();

        for (var i = 0; i < request.ColumnIds.Count; i++)
            columnsById[request.ColumnIds[i]].Position = i;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class ReorderColumnsCommandValidator : AbstractValidator<ReorderColumnsCommand>
{
    public ReorderColumnsCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnIds)
            .NotNull();

        RuleForEach(x => x.ColumnIds)
            .NotEmpty()
            .When(x => x.ColumnIds is not null);

        RuleFor(x => x.ColumnIds)
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
            .When(x => x.ColumnIds is not null)
            .WithMessage("Column ids must be unique.");
    }
}
