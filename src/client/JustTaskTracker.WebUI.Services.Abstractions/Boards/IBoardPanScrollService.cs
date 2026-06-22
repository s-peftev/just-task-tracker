using Microsoft.AspNetCore.Components;

namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

/// <summary>
/// Attaches horizontal pan-scroll behavior to the board page content area via JS interop.
/// </summary>
public interface IBoardPanScrollService
{
    bool IsAttached { get; }

    Task AttachAsync(ElementReference contentRef, ElementReference scrollRef);

    Task DetachAsync();
}
