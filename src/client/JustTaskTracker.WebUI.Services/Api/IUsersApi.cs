using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface IUsersApi
{
    [Get("/api/users")]
    Task<IApiResponse<ApiEnvelope<PagedList<UserForBoardLookupDto>>>> GetForBoardLookupAsync(
        Guid boardId,
        int pageNumber,
        int pageSize,
        [AliasAs("SearchOptions.Search")] string? searchOptionsSearch = null,
        CancellationToken ct = default);
}
