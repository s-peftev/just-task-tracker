using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardTaskAssigneePickerStore(IBoardApiService boardApiService) : IBoardTaskAssigneePickerStore
{
    public const int PageSize = 20;
    private const int SearchDebounceMilliseconds = 300;

    private Guid _boardId;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;

    public Guid? BoardId { get; private set; }
    public IReadOnlyList<BoardMemberDto> Members { get; private set; } = [];
    public PaginationMetadata Pagination { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public bool IsOpen { get; private set; }
    public bool IsLoading { get; private set; }
    public bool HasMoreMembers => Members.Count < Pagination.TotalCount;
    public bool IsLoadingMoreMembers { get; private set; }
    public string SearchText { get; private set; } = string.Empty;

    public event Action? StateChanged;

    public async Task OpenAsync(Guid boardId, CancellationToken ct = default)
    {
        _boardId = boardId;
        BoardId = boardId;
        SearchText = string.Empty;
        Members = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoadingMoreMembers = false;
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
        if (!IsOpen || !HasMoreMembers || IsLoadingMoreMembers || IsLoading)
            return;

        IsLoadingMoreMembers = true;
        NotifyStateChanged();

        try
        {
            await LoadPageAsync(CurrentPage + 1, replaceExisting: false, ct);
        }
        finally
        {
            IsLoadingMoreMembers = false;
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
        Members = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoading = false;
        IsLoadingMoreMembers = false;
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
            TextSearchOptions<BoardMemberSearchField>? searchOptions = string.IsNullOrWhiteSpace(SearchText)
                ? null
                : new TextSearchOptions<BoardMemberSearchField>(SearchText);

            var request = new GetBoardMembersRequest(searchOptions)
            {
                PageNumber = pageNumber,
                PageSize = PageSize,
            };

            var page = await boardApiService.GetBoardMembersAsync(
                _boardId,
                request,
                linkedCts.Token);

            var incoming = page.Items?.ToList() ?? [];

            Members = replaceExisting
                ? incoming
                : MergeMembers(Members, incoming);

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

    private static List<BoardMemberDto> MergeMembers(
        IReadOnlyList<BoardMemberDto> existing,
        IReadOnlyList<BoardMemberDto> incoming) =>
        existing
            .Concat(incoming)
            .GroupBy(member => member.User.Id)
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
