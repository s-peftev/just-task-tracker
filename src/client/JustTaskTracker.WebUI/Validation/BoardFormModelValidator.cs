using FluentValidation;

namespace JustTaskTracker.WebUI.Validation;

public class BoardFormModelValidator : AbstractValidator<BoardFormModel>
{
    public BoardFormModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Board name is required.")
            .MinimumLength(3).WithMessage("Board name must be at least 3 characters.")
            .MaximumLength(100).WithMessage("Board name must not exceed 100 characters.");
    }
}
