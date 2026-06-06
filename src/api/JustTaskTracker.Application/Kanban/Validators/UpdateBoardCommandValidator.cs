using FluentValidation;
using JustTaskTracker.Application.Kanban.Commands;

namespace JustTaskTracker.Application.Kanban.Validators;

public class UpdateBoardCommandValidator : AbstractValidator<UpdateBoardCommand>
{
    public UpdateBoardCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
