using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Columns;

public record UpdateColumnCommand(Guid BoardId, Guid ColumnId, string Name) : IRequest<Result>;

public class UpdateColumnCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IColumnRepository columnRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateColumnCommand, Result>
{
    public async Task<Result> Handle(UpdateColumnCommand request, CancellationToken ct)
    {
        var (column, userRole) = await columnRepository.GetColumnWithUserRoleAsync(request.ColumnId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageColumns(authorizedRole))
            return Result.Failure(GeneralErrors.Forbidden);

        if (column is null)
            return Result.Failure(GeneralErrors.NotFound);

        var name = request.Name.Trim();

        if (string.Equals(column.Name, name, StringComparison.OrdinalIgnoreCase))
            return Result.Success();

        if (await columnRepository.IsNameExistsAsync(request.BoardId, name, request.ColumnId, ct))
            return Result.Failure(ColumnsErrors.DuplicateName);

        column.Name = name;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class UpdateColumnCommandValidator : AbstractValidator<UpdateColumnCommand>
{
    public UpdateColumnCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("'Name' must not be empty.")
            .MaximumLength(ColumnFieldLengths.MaxNameLength);
    }
}
