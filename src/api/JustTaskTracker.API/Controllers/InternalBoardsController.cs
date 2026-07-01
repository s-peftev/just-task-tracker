using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Boards.Queries.Boards;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

/// <summary>
/// Internal API consumed exclusively by the Archival Function.
/// Protected by <see cref="InternalApiKeyMiddleware"/>; Azure AD auth is not required.
/// </summary>
[Route("internal/boards")]
[ApiController]
public class InternalBoardsController(ISender sender) : ControllerBase
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
}
