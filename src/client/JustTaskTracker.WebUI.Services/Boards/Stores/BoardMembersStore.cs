using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;

namespace JustTaskTracker.WebUI.Services.Boards.Stores;

internal sealed class BoardMembersStore(IBoardApiService boardApiService) : IBoardMembersStore
{
    public const int PageSize = 20;

    private Guid _boardId;
    private CancellationTokenSource? _loadCts;

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
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task OpenAsync(Guid boardId, BoardMemberRole userRole, CancellationToken ct = default)
    {
        _boardId = boardId;
        BoardId = boardId;
        UserRole = userRole;
        ActiveTab = BoardMembersOverlayTab.Members;
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
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;

        IsOpen = false;
        BoardId = null;
        Members = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoading = false;
        IsLoadingMoreMembers = false;
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
            var page = await boardApiService.GetBoardMembersAsync(
                _boardId,
                pageNumber,
                PageSize,
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

    private static List<BoardMemberDto> MergeMembers(
        IReadOnlyList<BoardMemberDto> existing,
        IReadOnlyList<BoardMemberDto> incoming) =>
        existing
            .Concat(incoming)
            .GroupBy(member => member.User.Id)
            .Select(group => group.First())
            .ToList();

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
