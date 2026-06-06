using FluentValidation;
using JustTaskTracker.Application.Kanban.Commands;

namespace JustTaskTracker.Application.Kanban.Validators;

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
