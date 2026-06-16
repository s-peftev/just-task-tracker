using FluentValidation;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands.BoardTasks;

public record CreateBoardTaskCommand(Guid ColumnId, string Title) : IRequest<Result<BoardTaskPreviewDto>>;

public class CreateBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBoardTaskCommand, Result<BoardTaskPreviewDto>>
{
    public async Task<Result<BoardTaskPreviewDto>> Handle(CreateBoardTaskCommand request, CancellationToken ct)
    {
        var currentUser = await userRepository.GetUserDtoByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUser is null)
            return Result<BoardTaskPreviewDto>.Failure(GeneralErrors.Unauthorized);

        var userRole = await columnRepository.GetUserRoleAsync(request.ColumnId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanManageTasks(authorizedRole))
            return Result<BoardTaskPreviewDto>.Failure(GeneralErrors.Forbidden);

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

        return Result<BoardTaskPreviewDto>.Success(new BoardTaskPreviewDto(
            task.Id,
            task.Title,
            task.Position));
    }
}

public class CreateBoardTaskCommandValidator : AbstractValidator<CreateBoardTaskCommand>
{
    public CreateBoardTaskCommandValidator()
    {
        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("'Title' must not be empty.")
            .MaximumLength(BoardTaskFieldLengths.MaxTitleLength);
    }
}
