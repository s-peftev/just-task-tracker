using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardTaskSearchStore(IBoardApiService boardApiService) : IBoardTaskSearchStore
{
    public const int PageSize = 20;
    private const int SearchDebounceMilliseconds = 300;

    private Guid _boardId;
    private Guid _columnId;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;

    public IReadOnlyList<BoardTaskLookupDto> Tasks { get; private set; } = [];
    public Guid? BoardId { get; private set; }
    public PaginationMetadata Pagination { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public string SearchText { get; private set; } = string.Empty;
    public bool IsOpen { get; private set; }
    public bool IsLoading { get; private set; }
    public bool HasMoreTasks => Tasks.Count < Pagination.TotalCount;
    public bool IsLoadingMoreTasks { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task OpenAsync(Guid boardId, Guid columnId, CancellationToken ct = default)
    {
        _boardId = boardId;
        _columnId = columnId;
        BoardId = boardId;
        IsOpen = true;
        NotifyStateChanged();

        await LoadInternalAsync(1, SearchText, replaceExisting: true, ct);
    }

    public async Task SetSearchAsync(string searchText, CancellationToken ct = default)
    {
        SearchText = searchText;
        NotifyStateChanged();

        if (!IsOpen)
            return;

        CancelSearchDebounce();

        var debounceCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _searchDebounceCts = debounceCts;

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, debounceCts.Token);
            await LoadInternalAsync(1, searchText, replaceExisting: true, ct);
        }
        catch (OperationCanceledException) when (debounceCts.IsCancellationRequested)
        {
            // Superseded by a newer keystroke or close.
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

    public async Task LoadMoreAsync(CancellationToken ct = default)
    {
        if (!IsOpen || !HasMoreTasks || IsLoadingMoreTasks)
            return;

        IsLoadingMoreTasks = true;
        NotifyStateChanged();

        try
        {
            await LoadInternalAsync(CurrentPage + 1, SearchText, replaceExisting: false, ct);
        }
        finally
        {
            IsLoadingMoreTasks = false;
            NotifyStateChanged();
        }
    }

    public void Close()
    {
        CancelSearchDebounce();

        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;

        IsOpen = false;
        BoardId = null;
        Tasks = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoading = false;
        IsLoadingMoreTasks = false;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    public void Reset()
    {
        Close();
        SearchText = string.Empty;
        NotifyStateChanged();
    }

    private async Task LoadInternalAsync(
        int pageNumber,
        string searchText,
        bool replaceExisting,
        CancellationToken ct)
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loadCts = linkedCts;

        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            TextSearchOptions<BoardTaskSearchField>? textSearch = string.IsNullOrWhiteSpace(searchText)
                ? null
                : new TextSearchOptions<BoardTaskSearchField>(searchText);

            var request = new GetBoardTasksLookupRequest(textSearch)
            {
                PageNumber = pageNumber,
                PageSize = PageSize,
            };

            var page = await boardApiService.GetBoardTasksLookupAsync(
                _boardId,
                _columnId,
                request,
                linkedCts.Token);

            var incoming = page.Items?.ToList() ?? [];

            Tasks = replaceExisting
                ? incoming
                : MergeTasks(Tasks, incoming);

            Pagination = page.Metadata;
            CurrentPage = page.Metadata.CurrentPage;
        }
        catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
        {
            // Superseded by a newer search/page request.
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (ReferenceEquals(_loadCts, linkedCts))
            {
                IsLoading = false;
                linkedCts.Dispose();
                _loadCts = null;
                NotifyStateChanged();
            }
            else
            {
                linkedCts.Dispose();
            }
        }
    }

    private static List<BoardTaskLookupDto> MergeTasks(
        IReadOnlyList<BoardTaskLookupDto> existing,
        IReadOnlyList<BoardTaskLookupDto> incoming)
    {
        var merged = existing.ToList();
        merged.AddRange(incoming);
        return merged;
    }

    private void CancelSearchDebounce()
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();
        _searchDebounceCts = null;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
