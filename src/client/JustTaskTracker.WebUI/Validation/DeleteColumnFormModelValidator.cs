using FluentValidation;
using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Validation;

public class DeleteColumnFormModelValidator : AbstractValidator<DeleteColumnFormModel>
{
    public DeleteColumnFormModelValidator(bool hasTasks, bool hasOtherColumns)
    {
        When(_ => hasTasks, () =>
        {
            RuleFor(x => x.TasksDisposition)
                .IsInEnum();

            When(x => x.TasksDisposition == DeleteColumnTasksDisposition.MoveToColumn, () =>
            {
                RuleFor(x => x.TargetColumnId)
                    .NotEmpty()
                    .WithMessage("Target column is required.")
                    .Must(_ => hasOtherColumns)
                    .WithMessage("There is no other column to move tasks to.");

                RuleFor(x => x.MovePlacement)
                    .NotNull()
                    .IsInEnum()
                    .WithMessage("Task placement is required.");
            });
        });
    }
}
