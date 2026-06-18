using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Boards.Enums.SearchFields;
using JustTaskTracker.WebUI.Domain.Boards.Requests;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Domain.Common.Searching;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardMembersStore(IBoardApiService boardApiService) : IBoardMembersStore
{
    public const int PageSize = 20;
    private const int SearchDebounceMilliseconds = 300;

    private Guid _boardId;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;

    public Guid? BoardId { get; private set; }
    public BoardMemberRole UserRole { get; private set; }
    public BoardMembersOverlayTab ActiveTab { get; private set; } = BoardMembersOverlayTab.Members;
    public IReadOnlyList<BoardMemberDto> Members { get; private set; } = [];
    public PaginationMetadata Pagination { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public bool IsOpen { get; private set; }
    public bool IsLoading { get; private set; }
    public bool HasMoreMembers => Members.Count < Pagination.TotalCount;
    public bool IsLoadingMoreMembers { get; private set; }
    public bool IsRemovingMember { get; private set; }
    public bool IsUpdatingMemberRole { get; private set; }
    public Guid? UpdatingMemberRoleUserId { get; private set; }
    public string SearchText { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task OpenAsync(Guid boardId, BoardMemberRole userRole, CancellationToken ct = default)
    {
        _boardId = boardId;
        BoardId = boardId;
        UserRole = userRole;
        ActiveTab = BoardMembersOverlayTab.Members;
        SearchText = string.Empty;
        Members = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoadingMoreMembers = false;
        ErrorMessage = null;
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

    public async Task SetActiveTabAsync(BoardMembersOverlayTab tab, CancellationToken ct = default)
    {
        if (ActiveTab == tab)
            return;

        ActiveTab = tab;
        NotifyStateChanged();

        if (tab == BoardMembersOverlayTab.Members && IsOpen)
            await RefreshAsync(ct);
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        if (!IsOpen)
            return;

        await LoadPageAsync(1, replaceExisting: true, ct);
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

    public async Task RemoveMemberAsync(Guid userId, CancellationToken ct = default)
    {
        if (!IsOpen)
            throw new InvalidOperationException("Board members overlay is not open.");

        IsRemovingMember = true;
        NotifyStateChanged();

        try
        {
            await boardApiService.DeleteBoardMemberAsync(_boardId, userId, ct);
            RemoveMemberFromList(userId);
        }
        finally
        {
            IsRemovingMember = false;
            NotifyStateChanged();
        }
    }

    public async Task UpdateMemberRoleAsync(Guid userId, BoardMemberRole role, CancellationToken ct = default)
    {
        if (!IsOpen)
            throw new InvalidOperationException("Board members overlay is not open.");

        IsUpdatingMemberRole = true;
        UpdatingMemberRoleUserId = userId;
        NotifyStateChanged();

        try
        {
            await boardApiService.UpdateBoardMemberAsync(
                _boardId,
                userId,
                new UpdateBoardMemberRequest(role),
                ct);

            UpdateMemberRoleInList(userId, role);
        }
        finally
        {
            IsUpdatingMemberRole = false;
            UpdatingMemberRoleUserId = null;
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
        IsRemovingMember = false;
        IsUpdatingMemberRole = false;
        UpdatingMemberRoleUserId = null;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    public void Reset()
    {
        Close();
        NotifyStateChanged();
    }

    private async Task LoadPageAsync(int pageNumber, bool replaceExisting, CancellationToken ct)
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
        catch (Exception ex)
        {
            if (replaceExisting)
                throw;

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

    private void RemoveMemberFromList(Guid userId)
    {
        Members = Members
            .Where(member => member.User.Id != userId)
            .ToList();

        Pagination = new PaginationMetadata
        {
            CurrentPage = Pagination.CurrentPage,
            PageSize = Pagination.PageSize,
            TotalCount = Math.Max(0, Pagination.TotalCount - 1),
        };

        NotifyStateChanged();
    }

    private void UpdateMemberRoleInList(Guid userId, BoardMemberRole role)
    {
        Members = Members
            .Select(member => member.User.Id == userId
                ? member with { Role = role }
                : member)
            .ToList();

        NotifyStateChanged();
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
