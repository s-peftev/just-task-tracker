using FluentValidation;
using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record UpdateBoardCommand(Guid BoardId, string Name) : IRequest<Result>;

public class UpdateBoardCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBoardCommand, Result>
{
    public async Task<Result> Handle(UpdateBoardCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(board is not null, userRole, BoardRolePermissions.CanRenameBoard) is { } failure)
            return failure;

        var name = request.Name.Trim();

        if (string.Equals(board!.Name, name, StringComparison.OrdinalIgnoreCase))
            return Result.Success();

        board.Name = name;

        await unitOfWork.SaveChangesAsync(ct);

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
