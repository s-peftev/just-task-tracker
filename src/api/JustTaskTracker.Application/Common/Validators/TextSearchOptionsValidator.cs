using FluentValidation;
using JustTaskTracker.Domain.Common.Searching;

namespace JustTaskTracker.Application.Common.Validators;

public sealed class TextSearchOptionsValidator<TField> : AbstractValidator<TextSearchOptions<TField>>
    where TField : struct, Enum
{
    public TextSearchOptionsValidator(int maxSearchLength)
    {
        When(x => !string.IsNullOrWhiteSpace(x.Search), () =>
        {
            RuleFor(x => x.Search)
                .MaximumLength(maxSearchLength);
        });

        When(x => x.SearchIn is not null, () =>
        {
            RuleForEach(x => x.SearchIn)
                .IsInEnum();
        });
    }
}
