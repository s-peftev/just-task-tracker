using FluentValidation;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Validators;
using JustTaskTracker.Domain.Boards.DTOs;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Searching;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries;

public record GetBoardsForCurrentUserQuery(TextSearchOptions<BoardSearchField>? TextSearchOptions) : PaginatedRequest, IRequest<Result<PagedList<BoardLookupDto>>>;

public class GetBoardsForCurrentUserQueryHandler(ICurrentUserAccessor currentUser, IBoardRepository boardRepository) 
    : IRequestHandler<GetBoardsForCurrentUserQuery, Result<PagedList<BoardLookupDto>>>
{
    public async Task<Result<PagedList<BoardLookupDto>>> Handle(GetBoardsForCurrentUserQuery request, CancellationToken ct)
    {
        var boards = await boardRepository.GetBoardsByUserAzureAOIAsync(
            currentUser.AzureAdObjectId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            request.TextSearchOptions,
            ct);

        return Result<PagedList<BoardLookupDto>>.Success(boards);
    }
}

public class GetBoardsForCurrentUserQueryValidator : AbstractValidator<GetBoardsForCurrentUserQuery>
{
    public GetBoardsForCurrentUserQueryValidator(ValidationSettings validationSettings)
    {
        var maxBoardNameSearchLength = validationSettings.Boards.MaxNameSearchLength;

        When(x => x.TextSearchOptions is not null, () =>
        {
            RuleFor(x => x.TextSearchOptions!)
                .SetValidator(new TextSearchOptionsValidator<BoardSearchField>(maxBoardNameSearchLength));
        });
    }
}
