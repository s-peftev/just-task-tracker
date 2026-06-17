using JustTaskTracker.WebUI.Domain.Auth.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;

namespace JustTaskTracker.WebUI.Domain.Auth.Requests;

public record GetUsersForBoardLookupRequest(TextSearchOptions<UserSearchField>? SearchOptions = null)
    : PaginatedRequest;
