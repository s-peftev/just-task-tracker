using FluentValidation;
using JustTaskTracker.WebUI.Domain.Calls;

namespace JustTaskTracker.WebUI.Validation;

public class CreateCallModelValidator : AbstractValidator<CreateCallModel>
{
    public CreateCallModelValidator()
    {
        RuleFor(x => x.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title is required.")
            .MaximumLength(CallFieldLengths.MaxTitleLength)
            .WithMessage($"Title must not exceed {CallFieldLengths.MaxTitleLength} characters.");

        RuleFor(x => x.Topic)
            .MaximumLength(CallFieldLengths.MaxTopicLength)
            .WithMessage($"Topic must not exceed {CallFieldLengths.MaxTopicLength} characters.");
    }
}
