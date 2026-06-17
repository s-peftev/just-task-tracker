using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Auth.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Auth.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Abstractions.Users;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardAddMemberStore(IUsersApiService usersApiService) : IBoardAddMemberStore
{
    public const int PageSize = 20;
    private const int SearchDebounceMilliseconds = 300;

    private Guid _boardId;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;

    public string SearchText { get; private set; } = string.Empty;
    public IReadOnlyList<UserForBoardLookupDto> Users { get; private set; } = [];
    public PaginationMetadata Pagination { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public bool IsAttached { get; private set; }
    public bool IsLoading { get; private set; }
    public bool HasMoreUsers => Users.Count < Pagination.TotalCount;
    public bool IsLoadingMoreUsers { get; private set; }

    public event Action? StateChanged;

    public void Attach(Guid boardId)
    {
        _boardId = boardId;
        IsAttached = true;
        NotifyStateChanged();
    }

    public async Task SetSearchAsync(string searchText, CancellationToken ct = default)
    {
        SearchText = searchText;
        NotifyStateChanged();

        if (!IsAttached)
            return;

        CancelSearchDebounce();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            CancelLoad();
            Users = [];
            Pagination = new PaginationMetadata();
            CurrentPage = 1;
            IsLoading = false;
            NotifyStateChanged();
            return;
        }

        var debounceCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _searchDebounceCts = debounceCts;

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, debounceCts.Token);
            await LoadInternalAsync(1, searchText, replaceExisting: true, ct);
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

    public async Task LoadMoreAsync(CancellationToken ct = default)
    {
        if (!IsAttached || !HasMoreUsers || IsLoadingMoreUsers || IsLoading || string.IsNullOrWhiteSpace(SearchText))
            return;

        IsLoadingMoreUsers = true;
        NotifyStateChanged();

        try
        {
            await LoadInternalAsync(CurrentPage + 1, SearchText, replaceExisting: false, ct);
        }
        finally
        {
            IsLoadingMoreUsers = false;
            NotifyStateChanged();
        }
    }

    public void Reset()
    {
        CancelSearchDebounce();
        CancelLoad();

        IsAttached = false;
        SearchText = string.Empty;
        Users = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoading = false;
        IsLoadingMoreUsers = false;
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
        NotifyStateChanged();

        try
        {
            var request = new GetUsersForBoardLookupRequest(
                new TextSearchOptions<UserSearchField>(searchText))
            {
                PageNumber = pageNumber,
                PageSize = PageSize,
            };

            var page = await usersApiService.GetUsersForBoardLookupAsync(
                _boardId,
                request,
                linkedCts.Token);

            var incoming = page.Items?.ToList() ?? [];

            Users = replaceExisting
                ? incoming
                : MergeUsers(Users, incoming);

            Pagination = page.Metadata;
            CurrentPage = page.Metadata.CurrentPage;
        }
        catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
        {
            // Superseded by a newer search/page request.
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

    private static List<UserForBoardLookupDto> MergeUsers(
        IReadOnlyList<UserForBoardLookupDto> existing,
        IReadOnlyList<UserForBoardLookupDto> incoming) =>
        existing
            .Concat(incoming)
            .GroupBy(user => user.Id)
            .Select(group => group.First())
            .ToList();

    private void CancelSearchDebounce()
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();
        _searchDebounceCts = null;
    }

    private void CancelLoad()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
