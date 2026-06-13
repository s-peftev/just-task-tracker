using FluentValidation;
using JustTaskTracker.Application.Boards.Authorization;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Application.Common.Models;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Commands;

public record UpdateBoardTaskCommand(
    Guid BoardId,
    Guid ColumnId,
    Guid BoardTaskId,
    PatchField<string> Title = default,
    PatchField<string?> Description = default,
    PatchField<Guid?> AssigneeId = default)
    : IRequest<Result>;

public class UpdateBoardTaskCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBoardTaskCommand, Result>
{
    public async Task<Result> Handle(UpdateBoardTaskCommand request, CancellationToken ct)
    {
        var (boardExists, userRole) = await boardRepository.GetUserBoardRoleAsync(
            request.BoardId,
            currentUserAccessor.AzureAdObjectId,
            ct);

        if (BoardRoleAuthorization.EnsureBoardAccess(boardExists, userRole, BoardRolePermissions.CanManageTasks) is { } failure)
            return failure;

        var boardTask = await boardTaskRepository.GetByBoardIdAndIdAsync(
            request.BoardId,
            request.BoardTaskId,
            ct);

        if (boardTask is null || boardTask.ColumnId != request.ColumnId)
            return Result.Failure(GeneralErrors.NotFound);

        var hasChanges = false;

        if (request.Title.IsSpecified)
        {
            var title = request.Title.Value!.Trim();

            if (!string.Equals(boardTask.Title, title, StringComparison.Ordinal))
            {
                boardTask.Title = title;
                hasChanges = true;
            }
        }

        if (request.Description.IsSpecified)
        {
            var description = string.IsNullOrWhiteSpace(request.Description.Value)
                ? null
                : request.Description.Value!.Trim();

            if (!string.Equals(boardTask.Description, description, StringComparison.Ordinal))
            {
                boardTask.Description = description;
                hasChanges = true;
            }
        }

        if (request.AssigneeId.IsSpecified)
        {
            var assigneeId = request.AssigneeId.Value;

            if (boardTask.AssigneeId != assigneeId)
            {
                if (assigneeId is { } assignedUserId
                    && await boardRepository.GetBoardMemberUserDtoAsync(request.BoardId, assignedUserId, ct) is null)
                {
                    return Result.Failure(BoardTasksErrors.AssigneeNotBoardMember);
                }

                boardTask.AssigneeId = assigneeId;
                hasChanges = true;
            }
        }

        if (!hasChanges)
            return Result.Success();

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class UpdateBoardTaskCommandValidator : AbstractValidator<UpdateBoardTaskCommand>
{
    public UpdateBoardTaskCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.BoardTaskId)
            .NotEmpty();

        RuleFor(x => x)
            .Must(command => command.Title.IsSpecified
                || command.Description.IsSpecified
                || command.AssigneeId.IsSpecified)
            .WithMessage("At least one field must be provided for update.");

        When(x => x.Title.IsSpecified, () =>
        {
            RuleFor(x => x.Title.Value)
                .Must(title => !string.IsNullOrWhiteSpace(title))
                .WithMessage("'Title' must not be empty.");

            When(x => !string.IsNullOrWhiteSpace(x.Title.Value), () =>
            {
                RuleFor(x => x.Title.Value)
                    .Must(title => title!.Trim().Length <= BoardTaskFieldLengths.MaxTitleLength)
                    .WithMessage($"'Title' must be {BoardTaskFieldLengths.MaxTitleLength} characters or fewer.");
            });
        });

        When(x => x.Description.IsSpecified && !string.IsNullOrWhiteSpace(x.Description.Value), () =>
        {
            RuleFor(x => x.Description.Value)
                .Must(description => description!.Trim().Length <= BoardTaskFieldLengths.MaxDescriptionLength)
                .WithMessage($"'Description' must be {BoardTaskFieldLengths.MaxDescriptionLength} characters or fewer.");
        });

        When(x => x.AssigneeId is { IsSpecified: true, Value: not null }, () =>
        {
            RuleFor(x => x.AssigneeId.Value)
                .NotEmpty();
        });
    }
}
