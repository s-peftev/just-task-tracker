using FluentValidation;
using JustTaskTracker.Application.Boards.Commands;

namespace JustTaskTracker.Application.Boards.Validators;

public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
