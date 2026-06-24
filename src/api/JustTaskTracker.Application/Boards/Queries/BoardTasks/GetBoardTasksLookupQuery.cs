using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Validators;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Domain.Common.Searching;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.BoardTasks;

public record GetBoardTasksLookupQuery(Guid BoardId, TextSearchOptions<BoardTaskSearchField>? SearchOptions) : PaginatedRequest, IRequest<Result<PagedList<BoardTaskLookupDto>>>;

public class GetBoardTasksLookupQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository)
    : IRequestHandler<GetBoardTasksLookupQuery, Result<PagedList<BoardTaskLookupDto>>>
{
    public async Task<Result<PagedList<BoardTaskLookupDto>>> Handle(GetBoardTasksLookupQuery request, CancellationToken ct)
    {
        var userRole = await boardRepository.GetUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<PagedList<BoardTaskLookupDto>>.Failure(GeneralErrors.Forbidden);

        var boardTasks = await boardTaskRepository.GetBoardTaskLookupListAsync(
            request.BoardId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            request.SearchOptions,
            ct);

        return Result<PagedList<BoardTaskLookupDto>>.Success(boardTasks);
    }
}

public class GetBoardTasksLookupQueryValidator : AbstractValidator<GetBoardTasksLookupQuery>
{
    public GetBoardTasksLookupQueryValidator(ValidationSettings validationSettings)
    {
        var maxBoardTaskTextSearchLength = validationSettings.BoardTasks!.MaxTextSearchLength;

        When(x => x.SearchOptions is not null, () =>
        {
            RuleFor(x => x.SearchOptions!)
                .SetValidator(new TextSearchOptionsValidator<BoardTaskSearchField>(maxBoardTaskTextSearchLength));
        });
    }
}