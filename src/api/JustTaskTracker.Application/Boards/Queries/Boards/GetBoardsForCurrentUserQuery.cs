using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Boards.Mappings;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Validators;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.Domain.Common.Pagination;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Searching;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.Boards;

public record GetBoardsForCurrentUserQuery(
    TextSearchOptions<BoardSearchField>? SearchOptions,
    bool? IsArchived = null,
    bool? IsOwned = null) : PaginatedRequest, IRequest<Result<PagedList<BoardLookupDto>>>;

public class GetBoardsForCurrentUserQueryHandler(
    ICurrentUserAccessor currentUser,
    IBoardRepository boardRepository,
    IBoardExportService boardExportService)
    : IRequestHandler<GetBoardsForCurrentUserQuery, Result<PagedList<BoardLookupDto>>>
{
    public async Task<Result<PagedList<BoardLookupDto>>> Handle(GetBoardsForCurrentUserQuery request, CancellationToken ct)
    {
        var boards = await boardRepository.GetBoardsByUserAzureAOIAsync(
            currentUser.AzureAdObjectId,
            request.PageNumber!.Value,
            request.PageSize!.Value,
            request.SearchOptions,
            request.IsArchived,
            request.IsOwned,
            ct);

        var archivedBoardIds = boards.Items
            .Where(board => board.IsArchived)
            .Select(board => board.Id)
            .ToList();

        var exportStatuses = await boardExportService
            .GetBoardListExportInfoAsync(archivedBoardIds, ct);

        var boardDtos = boards.Items.Select(board =>
        {
            var exportInfo = board.IsArchived && exportStatuses.TryGetValue(board.Id, out var status)
                ? status
                : null;

            return board.ToDto(exportInfo);
        });

        return Result<PagedList<BoardLookupDto>>.Success(new PagedList<BoardLookupDto>(boards.Metadata, boardDtos));
    }
}

public class GetBoardsForCurrentUserQueryValidator : AbstractValidator<GetBoardsForCurrentUserQuery>
{
    public GetBoardsForCurrentUserQueryValidator(ValidationSettings validationSettings)
    {
        var maxBoardNameSearchLength = validationSettings.Boards!.MaxBoardNameSearchLength;

        When(x => x.SearchOptions is not null, () =>
        {
            RuleFor(x => x.SearchOptions!)
                .SetValidator(new TextSearchOptionsValidator<BoardSearchField>(maxBoardNameSearchLength));
        });
    }
}
