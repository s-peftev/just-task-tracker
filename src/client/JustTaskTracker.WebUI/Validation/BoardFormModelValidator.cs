using FluentValidation;

namespace JustTaskTracker.WebUI.Validation;

public class BoardFormModelValidator : AbstractValidator<BoardFormModel>
{
    public BoardFormModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Board name is required.")
            .MaximumLength(100).WithMessage("Board name must not exceed 100 characters.");
    }
}
