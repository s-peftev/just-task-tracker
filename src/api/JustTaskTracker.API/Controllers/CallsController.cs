using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Calls.Commands;
using JustTaskTracker.Application.Calls.Queries;
using JustTaskTracker.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("calls")]
[ApiController]
[Authorize(Policy = AuthorizationPolicies.IsAppMember)]
public class CallsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActiveForBoard([FromQuery] Guid boardId, CancellationToken ct)
    {
        var result = await sender.Send(new ListActiveCallSessionsForBoardQuery(boardId), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCallCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new JoinCallCommand(id), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> End(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new EndCallCommand(id), ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }
}
