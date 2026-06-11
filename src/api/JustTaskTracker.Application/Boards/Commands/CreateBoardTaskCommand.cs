using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record CreateBoardTaskCommand(
    Guid BoardId,
    Guid ColumnId,
    string Title) 
    : IRequest<Result<BoardTaskLookupDto>>;

public class CreateBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBoardTaskCommand, Result<BoardTaskLookupDto>>
{
    public async Task<Result<BoardTaskLookupDto>> Handle(CreateBoardTaskCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (board is null)
            return Result<BoardTaskLookupDto>.Failure(GeneralErrors.NotFound);

        if (userRole is not { } role || !BoardRolePermissions.CanManageTasks(role))
            return Result<BoardTaskLookupDto>.Failure(GeneralErrors.Forbidden);

        if (!await columnRepository.ExistsByBoardIdAndIdAsync(request.BoardId, request.ColumnId, ct))
            return Result<BoardTaskLookupDto>.Failure(GeneralErrors.NotFound);

        var currentUser = await userRepository.GetUserDtoByAzureAOIAsync(
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (currentUser is null)
            return Result<BoardTaskLookupDto>.Failure(GeneralErrors.Unauthorized);

        var title = request.Title.Trim();

        var position = await boardTaskRepository.GetCountByColumnIdAsync(request.ColumnId, ct);

        var task = new BoardTask
        {
            ColumnId = request.ColumnId,
            Title = title,
            Position = position,
            ReporterId = currentUser.Id
        };

        boardTaskRepository.Add(task);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<BoardTaskLookupDto>.Success(new BoardTaskLookupDto(
            task.Id,
            task.Title,
            task.Position));
    }
}

public class CreateBoardTaskCommandValidator : AbstractValidator<CreateBoardTaskCommand>
{
    public CreateBoardTaskCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("'Title' must not be empty.")
            .MaximumLength(50);
    }
}
