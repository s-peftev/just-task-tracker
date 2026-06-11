using FluentValidation;

namespace JustTaskTracker.WebUI.Validation;

public class NameFormModelValidator : AbstractValidator<NameFormModel>
{
    public NameFormModelValidator(int maxLength, string fieldLabel)
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage($"{fieldLabel} is required.")
            .MaximumLength(maxLength)
            .WithMessage($"{fieldLabel} must not exceed {maxLength} characters.");
    }
}
