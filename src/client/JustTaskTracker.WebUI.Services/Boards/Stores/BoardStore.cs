using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardStore(IBoardApiService boardApiService) : IBoardStore
{
    public const int ActivePageSize = 11;
    public const int ArchivedPageSize = 12;
    private const int SearchDebounceMilliseconds = 300;

    private readonly Dictionary<Guid, BoardMemberRole> _roleCache = [];
    private readonly SectionData _active = new();
    private readonly SectionData _archived = new();

    public BoardListSectionState Active => _active.ToState();
    public BoardListSectionState Archived => _archived.ToState();

    public event Action? StateChanged;

    public Task LoadActiveAsync(int pageNumber, CancellationToken ct = default)
    {
        if (_active.IsLoaded && pageNumber == _active.CurrentPage && !_active.IsLoading)
            return Task.CompletedTask;

        return LoadInternalAsync(_active, isArchived: false, pageNumber, _active.SearchText, ct);
    }

    public Task LoadArchivedAsync(int pageNumber, CancellationToken ct = default)
    {
        if (_archived.IsLoaded && pageNumber == _archived.CurrentPage && !_archived.IsLoading)
            return Task.CompletedTask;

        return LoadInternalAsync(_archived, isArchived: true, pageNumber, _archived.SearchText, ct);
    }

    public Task SetActiveSearchAsync(string searchText, CancellationToken ct = default) =>
        SetSearchInternalAsync(_active, isArchived: false, searchText, ct);

    public Task SetArchivedSearchAsync(string searchText, CancellationToken ct = default) =>
        SetSearchInternalAsync(_archived, isArchived: true, searchText, ct);

    public Task RefreshAsync(CancellationToken ct = default) =>
        Task.WhenAll(
            LoadInternalAsync(_active, isArchived: false, _active.CurrentPage, _active.SearchText, ct),
            LoadInternalAsync(_archived, isArchived: true, _archived.CurrentPage, _archived.SearchText, ct));

    public BoardMemberRole? GetCachedRole(Guid boardId) =>
        _roleCache.TryGetValue(boardId, out var role) ? role : null;

    public void Reset()
    {
        CancelSearchDebounce(_active);
        CancelSearchDebounce(_archived);

        Interlocked.Increment(ref _active.LoadGeneration);
        Interlocked.Increment(ref _archived.LoadGeneration);

        _active.Reset();
        _archived.Reset();
        _roleCache.Clear();
        NotifyStateChanged();
    }

    private async Task SetSearchInternalAsync(
        SectionData section,
        bool isArchived,
        string searchText,
        CancellationToken ct)
    {
        section.SearchText = searchText;
        NotifyStateChanged();

        CancelSearchDebounce(section);

        var debounceCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        section.SearchDebounceCts = debounceCts;

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, debounceCts.Token);
            await LoadInternalAsync(section, isArchived, 1, searchText, ct);
        }
        catch (OperationCanceledException) when (debounceCts.IsCancellationRequested)
        {
            // Superseded by a newer keystroke or reset.
        }
        finally
        {
            if (ReferenceEquals(section.SearchDebounceCts, debounceCts))
            {
                debounceCts.Dispose();
                section.SearchDebounceCts = null;
            }
            else
            {
                debounceCts.Dispose();
            }
        }
    }

    private async Task LoadInternalAsync(
        SectionData section,
        bool isArchived,
        int pageNumber,
        string searchText,
        CancellationToken ct)
    {
        var generation = Interlocked.Increment(ref section.LoadGeneration);

        section.IsLoading = true;
        section.ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            TextSearchOptions<BoardSearchField>? textSearch = string.IsNullOrWhiteSpace(searchText)
                ? null
                : new TextSearchOptions<BoardSearchField>(searchText);

            var request = new GetBoardsForCurrentUserRequest(textSearch, isArchived)
            {
                PageNumber = pageNumber,
                PageSize = isArchived ? ArchivedPageSize : ActivePageSize,
            };
            var page = await boardApiService.GetMyBoardsAsync(request, ct);

            if (generation != section.LoadGeneration)
                return;

            section.Boards = page.Items;
            section.Pagination = page.Metadata;
            section.CurrentPage = page.Metadata.CurrentPage;
            section.IsLoaded = true;

            foreach (var board in page.Items)
                _roleCache[board.Id] = board.UserRole;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Component disposed or navigation away.
        }
        catch (Exception ex)
        {
            if (generation != section.LoadGeneration)
                return;

            section.ErrorMessage = ex.Message;
        }
        finally
        {
            if (generation == section.LoadGeneration)
            {
                section.IsLoading = false;
                NotifyStateChanged();
            }
        }
    }

    private static void CancelSearchDebounce(SectionData section)
    {
        section.SearchDebounceCts?.Cancel();
        section.SearchDebounceCts?.Dispose();
        section.SearchDebounceCts = null;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();

    private sealed class SectionData
    {
        public int LoadGeneration;
        public CancellationTokenSource? SearchDebounceCts;
        public IReadOnlyList<BoardLookupDto> Boards { get; set; } = [];
        public PaginationMetadata Pagination { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public string SearchText { get; set; } = string.Empty;
        public bool IsLoading { get; set; }
        public bool IsLoaded { get; set; }
        public string? ErrorMessage { get; set; }

        public void Reset()
        {
            Boards = [];
            Pagination = new PaginationMetadata();
            CurrentPage = 1;
            SearchText = string.Empty;
            IsLoading = false;
            IsLoaded = false;
            ErrorMessage = null;
        }

        public BoardListSectionState ToState() => new()
        {
            Boards = Boards,
            Pagination = Pagination,
            CurrentPage = CurrentPage,
            SearchText = SearchText,
            IsLoading = IsLoading,
            IsLoaded = IsLoaded,
            ErrorMessage = ErrorMessage,
        };
    }
}
