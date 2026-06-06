using JustTaskTracker.WebUI.Domain.Common;
using JustTaskTracker.WebUI.Domain.Kanban;
using JustTaskTracker.WebUI.Domain.Kanban.Enums;
using JustTaskTracker.WebUI.Services.Abstractions.Kanban;

namespace JustTaskTracker.WebUI.Services.Kanban.Stores;

/// <summary>
/// Scoped store for the dashboard boards list. Loads pages on demand and keeps a
/// per-board role cache so board-scoped pages can resolve permissions locally.
/// </summary>
internal sealed class BoardStore(IBoardApiService boardApiService) : IBoardStore
{
    public const int PageSize = 23;

    private readonly Dictionary<Guid, BoardMemberRole> _roleCache = [];

    public IReadOnlyList<BoardLookupDto> Boards { get; private set; } = [];
    public PaginationMetadata Pagination { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public Task LoadAsync(int pageNumber, CancellationToken ct = default)
    {
        if (IsLoaded && pageNumber == CurrentPage && !IsLoading)
            return Task.CompletedTask;

        return LoadInternalAsync(pageNumber, ct);
    }

    public Task RefreshAsync(CancellationToken ct = default) =>
        LoadInternalAsync(CurrentPage, ct);

    public BoardMemberRole? GetCachedRole(Guid boardId) =>
        _roleCache.TryGetValue(boardId, out var role) ? role : null;

    public void Reset()
    {
        Boards = [];
        Pagination = new PaginationMetadata();
        CurrentPage = 1;
        IsLoading = false;
        IsLoaded = false;
        ErrorMessage = null;
        _roleCache.Clear();
        NotifyStateChanged();
    }

    private async Task LoadInternalAsync(int pageNumber, CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var page = await boardApiService.GetMyBoardsAsync(pageNumber, PageSize, ct);

            Boards = page.Items;
            Pagination = page.Metadata;
            CurrentPage = page.Metadata.CurrentPage;
            IsLoaded = true;

            foreach (var board in page.Items)
                _roleCache[board.Id] = board.UserRole;
        }
        catch (Exception ex)
        {
            // ApiServiceException carries the server message; other exceptions (network, etc.) fall through here too.
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
