using JustTaskTracker.WebUI.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;

namespace JustTaskTracker.WebUI.Domain.Boards.Requests;

public record GetBoardTasksLookupRequest(TextSearchOptions<BoardTaskSearchField>? SearchOptions = null)
    : PaginatedRequest;
