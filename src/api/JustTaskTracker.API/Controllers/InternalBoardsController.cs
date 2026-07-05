using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Boards.Queries.Boards;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Messaging;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

/// <summary>
/// Internal API consumed exclusively by the Archival Function.
/// Protected by <see cref="InternalApiKeyMiddleware"/>; Azure AD auth is not required.
/// </summary>
[Route("internal/boards")]
[ApiController]
public class InternalBoardsController(
    ISender sender,
    IBoardExportStatusNotifier exportStatusNotifier) : ControllerBase
{
    [HttpPost("{id:guid}/export-data")]
    public async Task<IActionResult> GetExportData(
        Guid id,
        [FromBody] BoardExportOptions exportOptions,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetBoardExportDataQuery(id, exportOptions), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/export-status-notify")]
    public async Task<IActionResult> NotifyExportStatusChanged(
        Guid id,
        [FromBody] BoardExportStatusChangedNotification notification,
        CancellationToken ct)
    {
        if (id != notification.BoardId)
            return GeneralErrors.InvalidRequest.CreateErrorResponse();

        await exportStatusNotifier.NotifyExportStatusChangedAsync(notification, ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/re-export-status-notify")]
    public async Task<IActionResult> NotifyReExportStatusChanged(
        Guid id,
        [FromBody] BoardExportStatusChangedNotification notification,
        CancellationToken ct)
    {
        if (id != notification.BoardId)
            return GeneralErrors.InvalidRequest.CreateErrorResponse();

        await exportStatusNotifier.NotifyReExportStatusChangedAsync(notification, ct);

        return NoContent();
    }
}
