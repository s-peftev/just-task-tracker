using JustTaskTracker.WebUI.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;

namespace JustTaskTracker.WebUI.Domain.Boards.Requests;

public record GetBoardsForCurrentUserRequest(
    TextSearchOptions<BoardSearchField>? TextSearchOptions = null,
    bool? IsArchived = null,
    bool? IsOwned = null)
    : PaginatedRequest;
