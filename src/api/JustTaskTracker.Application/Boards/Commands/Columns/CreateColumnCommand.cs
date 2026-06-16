using FluentValidation;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.Columns;

public record CreateColumnCommand(Guid BoardId, string Name) : IRequest<Result<ColumnDto>>;

public class CreateColumnCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateColumnCommand, Result<ColumnDto>>
{
    public async Task<Result<ColumnDto>> Handle(CreateColumnCommand request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageColumns(authorizedRole))
            return Result<ColumnDto>.Failure(GeneralErrors.Forbidden);

        var name = request.Name.Trim();
        var existingNames = await columnRepository.GetNameListByBoardIdAsync(request.BoardId, ct);

        if (existingNames.Any(existingName =>
                string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<ColumnDto>.Failure(ColumnsErrors.DuplicateName);
        }

        var column = new Column
        {
            BoardId = request.BoardId,
            Name = name,
            Position = existingNames.Count
        };

        columnRepository.Add(column);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<ColumnDto>.Success(new ColumnDto(
            column.Id,
            column.Name,
            column.Position,
            []));
    }
}

public class CreateColumnCommandValidator : AbstractValidator<CreateColumnCommand>
{
    public CreateColumnCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("'Name' must not be empty.")
            .MaximumLength(ColumnFieldLengths.MaxNameLength);
    }
}
