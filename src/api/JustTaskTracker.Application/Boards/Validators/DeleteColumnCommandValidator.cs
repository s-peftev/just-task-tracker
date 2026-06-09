using FluentValidation;
using JustTaskTracker.Application.Boards.Commands;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Validators;

public class DeleteColumnCommandValidator : AbstractValidator<DeleteColumnCommand>
{
    public DeleteColumnCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnId)
            .NotEmpty();

        RuleFor(x => x.TasksDisposition)
            .IsInEnum();

        When(x => x.TasksDisposition == DeleteColumnTasksDisposition.DeleteWithColumn, () =>
        {
            RuleFor(x => x.TargetColumnId)
                .Null()
                .WithMessage("'Target Column Id' must be null when deleting tasks with the column.");

            RuleFor(x => x.MovePlacement)
                .Null()
                .WithMessage("'Move Placement' must be null when deleting tasks with the column.");
        });

        When(x => x.TasksDisposition == DeleteColumnTasksDisposition.MoveToColumn, () =>
        {
            RuleFor(x => x.TargetColumnId)
                .NotEmpty()
                .WithMessage("'Target Column Id' is required when moving tasks to another column.");

            RuleFor(x => x.MovePlacement)
                .NotNull()
                .IsInEnum()
                .WithMessage("'Move Placement' is required when moving tasks to another column.");

            RuleFor(x => x)
                .Must(x => x.TargetColumnId != x.ColumnId)
                .WithMessage("Tasks cannot be moved to the column being deleted.");
        });
    }
}
