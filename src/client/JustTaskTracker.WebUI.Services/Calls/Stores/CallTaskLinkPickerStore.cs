using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Abstractions.Calls;

namespace JustTaskTracker.WebUI.Services.Calls.Stores;

internal sealed class CallTaskLinkPickerStore(IBoardApiService boardApiService) : ICallTaskLinkPickerStore
{
    public const int PageSize = 20;
    private const int SearchDebounceMilliseconds = 300;

    private Guid _boardId;
    private Guid _columnId;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;
    private readonly List<Guid> _selectedTaskIds = [];
    // Survives search filtering re-fetching Tasks (a new search replaces the whole page), so an
    // already-linked task never appears to vanish from the "Linked" section just because the
    // current search text no longer matches it.
    private readonly Dictionary<Guid, BoardTaskLookupDto> _linkedTaskDetails = [];

    public Guid? BoardId { get; private set; }
    public IReadOnlyList<BoardTaskLookupDto> Tasks { get; private set; } = [];
    public PaginationMetadata Pagination { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public bool IsOpen { get; private set; }
    public bool IsLoading { get; private set; }
    public bool HasMoreTasks => Tasks.Count < Pagination.TotalCount;
    public bool IsLoadingMoreTasks { get; private set; }
    public string SearchText { get; private set; } = string.Empty;

    public IReadOnlyList<Guid> SelectedTaskIds => _selectedTaskIds;

    public IReadOnlyList<BoardTaskLookupDto> LinkedTasks =>
        _selectedTaskIds
            .Select(id => _linkedTaskDetails.GetValueOrDefault(id))
            .Where(task => task is not null)
            .Select(task => task!)
            .ToList();

    public event Action? StateChanged;

    public void Reset()
    {
        // A leftover IsOpen=true from a picker the user never explicitly closed (e.g. they
        // cancelled the whole create-call dialog instead) must not resurrect the overlay the next
        // time a fresh dialog session starts.
        if (IsOpen)
            Close();

        _selectedTaskIds.Clear();
        _linkedTaskDetails.Clear();
        NotifyStateChanged();
    }

    public async Task OpenAsync(Guid boardId, Guid columnId, CancellationToken ct = default)
    {
        _boardId = boardId;
        _columnId = columnId;
        BoardId = boardId;
        SearchText = string.Empty;
        Tasks = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoadingMoreTasks = false;
        IsOpen = true;
        NotifyStateChanged();

        try
        {
            await LoadPageAsync(1, replaceExisting: true, ct);
        }
        catch
        {
            Close();
            throw;
        }
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
            await LoadPageAsync(1, replaceExisting: true, ct);
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
        if (!IsOpen || !HasMoreTasks || IsLoadingMoreTasks || IsLoading)
            return;

        IsLoadingMoreTasks = true;
        NotifyStateChanged();

        try
        {
            await LoadPageAsync(CurrentPage + 1, replaceExisting: false, ct);
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
        SearchText = string.Empty;
        Tasks = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoading = false;
        IsLoadingMoreTasks = false;
        NotifyStateChanged();
    }

    public bool IsSelected(Guid taskId) => _selectedTaskIds.Contains(taskId);

    public void Toggle(BoardTaskLookupDto task)
    {
        if (_selectedTaskIds.Remove(task.Id))
        {
            _linkedTaskDetails.Remove(task.Id);
        }
        else
        {
            _selectedTaskIds.Add(task.Id);
            _linkedTaskDetails[task.Id] = task;
        }

        NotifyStateChanged();
    }

    private async Task LoadPageAsync(int pageNumber, bool replaceExisting, CancellationToken ct)
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loadCts = linkedCts;

        IsLoading = true;
        NotifyStateChanged();

        try
        {
            TextSearchOptions<BoardTaskSearchField>? searchOptions = string.IsNullOrWhiteSpace(SearchText)
                ? null
                : new TextSearchOptions<BoardTaskSearchField>(SearchText);

            var request = new GetBoardTasksLookupRequest(searchOptions)
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
            // Superseded by a newer page request or close.
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
        IReadOnlyList<BoardTaskLookupDto> incoming) =>
        existing
            .Concat(incoming)
            .GroupBy(task => task.Id)
            .Select(group => group.First())
            .ToList();

    private void CancelSearchDebounce()
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();
        _searchDebounceCts = null;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
