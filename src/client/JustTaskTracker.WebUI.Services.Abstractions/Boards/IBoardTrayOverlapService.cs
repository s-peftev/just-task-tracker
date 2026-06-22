using Microsoft.AspNetCore.Components;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Observes horizontal overlap between board columns and the footer tray via JS interop.
/// </summary>
public interface IBoardTrayOverlapService
{
    bool IsAttached { get; }

    IReadOnlySet<Guid> OverlappingColumnIds { get; }

    event Action? OverlapChanged;

    Task AttachAsync(ElementReference scrollBodyRef, ElementReference footerPanelRef);

    Task DetachAsync();

    Task ReleaseResourcesAsync();

    Task RefreshAsync();
}
