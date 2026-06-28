using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

public sealed class BoardListSectionState
{
    public IReadOnlyList<BoardLookupDto> Boards { get; init; } = [];
    public PaginationMetadata Pagination { get; init; } = new();
    public int CurrentPage { get; init; } = 1;
    public string SearchText { get; init; } = string.Empty;
    public bool IsLoading { get; init; }
    public bool IsLoaded { get; init; }
    public string? ErrorMessage { get; init; }
}
