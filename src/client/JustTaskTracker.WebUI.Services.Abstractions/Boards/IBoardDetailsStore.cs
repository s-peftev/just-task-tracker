using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Boards.Enums;
using JustTaskTracker.WebUI.Domain.Boards.Requests;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Scoped store for a single board details page (columns, tasks, permissions).
/// </summary>
public interface IBoardDetailsStore
{
    Guid? BoardId { get; }
    BoardDetailsDto? Board { get; }
    bool IsLoading { get; }
    string? ErrorMessage { get; }

    event Action? StateChanged;

    Task LoadAsync(Guid boardId, CancellationToken ct = default);

    Task<ColumnDto> CreateColumnAsync(string name, CancellationToken ct = default);

    void UpdateBoardName(string name);

    void UpdateColumnName(Guid columnId, string name);

    Task DeleteColumnAsync(Guid columnId, DeleteColumnRequest request, CancellationToken ct = default);

    void Reset();
}
