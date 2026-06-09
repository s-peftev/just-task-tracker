using FluentValidation;
using JustTaskTracker.Application.Boards.Queries;
using JustTaskTracker.Application.Common.Validators;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;

namespace JustTaskTracker.Application.Boards.Validators;

public class GetBoardsForCurrentUserQueryValidator : AbstractValidator<GetBoardsForCurrentUserQuery>
{
    private const int MaxBoardNameSearchLength = 100;

    public GetBoardsForCurrentUserQueryValidator()
    {
        When(x => x.TextSearchOptions is not null, () =>
        {
            RuleFor(x => x.TextSearchOptions!)
                .SetValidator(new TextSearchOptionsValidator<BoardSearchField>(MaxBoardNameSearchLength));
        });
    }
}
