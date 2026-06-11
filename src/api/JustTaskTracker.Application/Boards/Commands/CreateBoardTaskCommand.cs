using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record CreateBoardTaskCommand(
    Guid BoardId,
    Guid ColumnId,
    string Title,
    string? Description,
    Guid? AssigneeId) 
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

        UserDto? assigneeDto = null;

        if (request.AssigneeId is { } assigneeId)
        {
            assigneeDto = await boardRepository.GetBoardMemberUserDtoAsync(
                request.BoardId,
                assigneeId,
                ct);

            if (assigneeDto is null)
                return Result<BoardTaskDto>.Failure(BoardTasksErrors.AssigneeNotBoardMember);
        }

        var title = request.Title.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var position = await boardTaskRepository.GetCountByColumnIdAsync(request.ColumnId, ct);

        var task = new BoardTask
        {
            ColumnId = request.ColumnId,
            Title = title,
            Description = description,
            Position = position,
            ReporterId = currentUser.Id,
            AssigneeId = request.AssigneeId
        };

        boardTaskRepository.Add(task);

        await unitOfWork.SaveChangesAsync(ct);

        return Result<BoardTaskDto>.Success(new BoardTaskDto(
            task.Id,
            task.Title,
            task.Position,
            task.CreatedAtUtc,
            currentUser,
            task.Description,
            assigneeDto));
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

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x.AssigneeId)
            .Must(id => id != Guid.Empty)
            .When(x => x.AssigneeId.HasValue);
    }
}
