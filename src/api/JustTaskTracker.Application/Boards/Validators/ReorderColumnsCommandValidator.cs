using FluentValidation;
using JustTaskTracker.Application.Boards.Commands;

namespace JustTaskTracker.Application.Boards.Validators;

public class ReorderColumnsCommandValidator : AbstractValidator<ReorderColumnsCommand>
{
    public ReorderColumnsCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.ColumnIds)
            .NotNull();

        RuleForEach(x => x.ColumnIds)
            .NotEmpty()
            .When(x => x.ColumnIds is not null);

        RuleFor(x => x.ColumnIds)
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
            .When(x => x.ColumnIds is not null)
            .WithMessage("Column ids must be unique.");
    }
}
