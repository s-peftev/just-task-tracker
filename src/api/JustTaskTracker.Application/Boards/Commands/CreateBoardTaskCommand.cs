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
    : IRequest<Result<BoardTaskDto>>;

public class CreateBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBoardTaskCommand, Result<BoardTaskDto>>
{
    public async Task<Result<BoardTaskDto>> Handle(CreateBoardTaskCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (board is null)
            return Result<BoardTaskDto>.Failure(GeneralErrors.NotFound);

        if (userRole is not { } role || !BoardRolePermissions.CanManageTasks(role))
            return Result<BoardTaskDto>.Failure(GeneralErrors.Forbidden);

        if (!await columnRepository.ExistsByBoardIdAndIdAsync(request.BoardId, request.ColumnId, ct))
            return Result<BoardTaskDto>.Failure(GeneralErrors.NotFound);

        var currentUser = await userRepository.GetUserDtoByAzureAOIAsync(
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (currentUser is null)
            return Result<BoardTaskDto>.Failure(GeneralErrors.Unauthorized);

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

        return Result<BoardTaskDto>.Success(new BoardTaskDto(
            task.Id,
            task.Title,
            task.Position,
            task.CreatedAtUtc,
            currentUser,
            task.Description));
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
