using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

/// <summary>
/// Scoped store for the boards list page. Loads pages on demand and keeps a
/// per-board role cache so board-scoped pages can resolve permissions locally.
/// </summary>
internal sealed class BoardStore(IBoardApiService boardApiService) : IBoardStore
{
    public const int PageSize = 23;
    private const int SearchDebounceMilliseconds = 300;

    private readonly Dictionary<Guid, BoardMemberRole> _roleCache = [];
    private int _loadGeneration;
    private CancellationTokenSource? _searchDebounceCts;

    public IReadOnlyList<BoardLookupDto> Boards { get; private set; } = [];
    public PaginationMetadata Pagination { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public string SearchText { get; private set; } = string.Empty;
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public Task LoadAsync(int pageNumber, CancellationToken ct = default)
    {
        if (IsLoaded && pageNumber == CurrentPage && !IsLoading)
            return Task.CompletedTask;

        return LoadInternalAsync(pageNumber, SearchText, ct);
    }

    public async Task SetSearchAsync(string searchText, CancellationToken ct = default)
    {
        SearchText = searchText;
        NotifyStateChanged();

        CancelSearchDebounce();

        var debounceCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _searchDebounceCts = debounceCts;

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, debounceCts.Token);
            await LoadInternalAsync(1, searchText, ct);
        }
        catch (OperationCanceledException) when (debounceCts.IsCancellationRequested)
        {
            // Superseded by a newer keystroke or reset.
        }
        finally
        {
            if (ReferenceEquals(_searchDebounceCts, debounceCts))
            {
                debounceCts.Dispose();
                _searchDebounceCts = null;
            }
            else
            {
                debounceCts.Dispose();
            }
        }
    }

    public Task RefreshAsync(CancellationToken ct = default) =>
        LoadInternalAsync(CurrentPage, SearchText, ct);

    public BoardMemberRole? GetCachedRole(Guid boardId) =>
        _roleCache.TryGetValue(boardId, out var role) ? role : null;

    public void Reset()
    {
        CancelSearchDebounce();

        Interlocked.Increment(ref _loadGeneration);

        Boards = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        SearchText = string.Empty;
        IsLoading = false;
        IsLoaded = false;
        ErrorMessage = null;
        _roleCache.Clear();
        NotifyStateChanged();
    }

    private async Task LoadInternalAsync(int pageNumber, string searchText, CancellationToken ct)
    {
        var generation = Interlocked.Increment(ref _loadGeneration);

        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            TextSearchOptions<BoardSearchField>? textSearch = string.IsNullOrWhiteSpace(searchText)
                ? null
                : new TextSearchOptions<BoardSearchField>(searchText);

            var request = new GetBoardsForCurrentUserRequest(textSearch)
            {
                PageNumber = pageNumber,
                PageSize = PageSize,
            };
            var page = await boardApiService.GetMyBoardsAsync(request, ct);

            if (generation != _loadGeneration)
                return;

            Boards = page.Items;
            Pagination = page.Metadata;
            CurrentPage = page.Metadata.CurrentPage;
            IsLoaded = true;

            foreach (var board in page.Items)
                _roleCache[board.Id] = board.UserRole;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Component disposed or navigation away.
        }
        catch (Exception ex)
        {
            if (generation != _loadGeneration)
                return;

            // ApiServiceException carries the server message; other exceptions (network, etc.) fall through here too.
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (generation == _loadGeneration)
            {
                IsLoading = false;
                NotifyStateChanged();
            }
        }
    }

    private void CancelSearchDebounce()
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();
        _searchDebounceCts = null;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
